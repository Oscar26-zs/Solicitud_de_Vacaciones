using FluentValidation;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>
/// Handler del caso de uso CU-07: cancela una solicitud Pending, libera el saldo
/// pendiente y registra el evento en historial.
/// </summary>
public sealed class CancelarSolicitudCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;
    private readonly IRepositorioSaldoEmpleado _saldos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProveedorTiempoCorporativo _proveedorTiempo;
    private readonly CancelarSolicitudCommandValidator _validator;

    public CancelarSolicitudCommandHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioSaldoEmpleado saldos,
        IUnitOfWork unitOfWork,
        IProveedorTiempoCorporativo proveedorTiempo,
        CancelarSolicitudCommandValidator validator)
    {
        _solicitudes = solicitudes;
        _saldos = saldos;
        _unitOfWork = unitOfWork;
        _proveedorTiempo = proveedorTiempo;
        _validator = validator;
    }

    public async Task HandleAsync(CancelarSolicitudCommand comando, CancellationToken cancellationToken = default)
    {
        var fechaActual = _proveedorTiempo.ObtenerFechaActualCorporativa();

        var validacion = await _validator.ValidateAsync(comando, cancellationToken);
        if (!validacion.IsValid)
        {
            throw new ValidationException(validacion.Errors);
        }

        var solicitud = await _solicitudes.ObtenerPorIdAsync(comando.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException();

        if (solicitud.EmpleadoId != comando.EmpleadoId)
        {
            throw new AccesoNoPermitidoException("Solo el dueño de la solicitud puede cancelarla.");
        }

        solicitud.Cancelar(fechaActual);

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