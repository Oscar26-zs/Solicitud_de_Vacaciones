using FluentValidation;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>
/// Handler del caso de uso CU-12: rechaza una solicitud Pending con comentario
/// obligatorio (1..500), libera el saldo pendiente y registra el evento.
/// </summary>
public sealed class RechazarSolicitudCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;
    private readonly IRepositorioSaldoEmpleado _saldos;
    private readonly IRepositorioEmpleado _empleados;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProveedorTiempoCorporativo _proveedorTiempo;
    private readonly RechazarSolicitudCommandValidator _validator;

    public RechazarSolicitudCommandHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioSaldoEmpleado saldos,
        IRepositorioEmpleado empleados,
        IUnitOfWork unitOfWork,
        IProveedorTiempoCorporativo proveedorTiempo,
        RechazarSolicitudCommandValidator validator)
    {
        _solicitudes = solicitudes;
        _saldos = saldos;
        _empleados = empleados;
        _unitOfWork = unitOfWork;
        _proveedorTiempo = proveedorTiempo;
        _validator = validator;
    }

    public async Task HandleAsync(RechazarSolicitudCommand comando, CancellationToken cancellationToken = default)
    {
        var fechaActual = _proveedorTiempo.ObtenerFechaActualCorporativa();

        var validacion = await _validator.ValidateAsync(comando, cancellationToken);
        if (!validacion.IsValid)
        {
            throw new ValidationException(validacion.Errors);
        }

        var aprobador = await _empleados.ObtenerPorIdAsync(comando.AprobadorEmpleadoId, cancellationToken)
            ?? throw new EmpleadoNoEncontradoException("Aprobador no encontrado.");

        if (!aprobador.EstaActivo)
        {
            throw new AprobadorInactivoException();
        }

        var solicitud = await _solicitudes.ObtenerPorIdAsync(comando.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException();

        if (solicitud.EmpleadoId == comando.AprobadorEmpleadoId)
        {
            throw new AutoAprobacionNoPermitidaException();
        }

        solicitud.Rechazar(comando.AprobadorEmpleadoId, comando.Comentario, fechaActual);

        var saldo = await _saldos.ObtenerPorEmpleadoIdAsync(solicitud.EmpleadoId, cancellationToken);
        if (saldo is not null)
        {
            saldo.LiberarSaldoPendiente(solicitud.DiasRequeridos, fechaActual);
            await _saldos.ActualizarAsync(saldo, cancellationToken);
        }

        await _solicitudes.ActualizarAsync(solicitud, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}