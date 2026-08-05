using FluentValidation;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>
/// Handler del caso de uso CU-11: aprueba una solicitud Pending. Valida
/// anti-auto-aprobación, aprobador activo, estado Pending y vuelve a verificar
/// saldo disponible (pudo haber cambiado desde la creación).
/// </summary>
public sealed class AprobarSolicitudCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;
    private readonly IRepositorioSaldoEmpleado _saldos;
    private readonly IRepositorioEmpleado _empleados;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProveedorTiempoCorporativo _proveedorTiempo;
    private readonly AprobarSolicitudCommandValidator _validator;

    public AprobarSolicitudCommandHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioSaldoEmpleado saldos,
        IRepositorioEmpleado empleados,
        IUnitOfWork unitOfWork,
        IProveedorTiempoCorporativo proveedorTiempo,
        AprobarSolicitudCommandValidator validator)
    {
        _solicitudes = solicitudes;
        _saldos = saldos;
        _empleados = empleados;
        _unitOfWork = unitOfWork;
        _proveedorTiempo = proveedorTiempo;
        _validator = validator;
    }

    public async Task HandleAsync(AprobarSolicitudCommand comando, CancellationToken cancellationToken = default)
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

        var saldo = await _saldos.ObtenerPorEmpleadoIdAsync(solicitud.EmpleadoId, cancellationToken)
            ?? throw new SaldoNoEncontradoException("No se encontró un saldo registrado para el empleado.");

        // Re-verifica saldo disponible antes de aprobar (RN-03).
        if (saldo.SaldoDisponible < solicitud.DiasRequeridos)
        {
            throw new SaldoInsuficienteException();
        }

        solicitud.Aprobar(comando.AprobadorEmpleadoId, fechaActual);
        saldo.DescontarSaldo(solicitud.DiasRequeridos, fechaActual);

        await _solicitudes.ActualizarAsync(solicitud, cancellationToken);
        await _saldos.ActualizarAsync(saldo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}