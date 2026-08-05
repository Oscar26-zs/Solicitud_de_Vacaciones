using FluentValidation;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>
/// Handler del caso de uso CU-14: cancela una solicitud Approved solo si su fecha
/// de inicio es futura (RN-04), restaurando el saldo consumido al disponible.
/// </summary>
public sealed class CancelarAprobadaCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;
    private readonly IRepositorioSaldoEmpleado _saldos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProveedorTiempoCorporativo _proveedorTiempo;
    private readonly CancelarAprobadaCommandValidator _validator;

    public CancelarAprobadaCommandHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioSaldoEmpleado saldos,
        IUnitOfWork unitOfWork,
        IProveedorTiempoCorporativo proveedorTiempo,
        CancelarAprobadaCommandValidator validator)
    {
        _solicitudes = solicitudes;
        _saldos = saldos;
        _unitOfWork = unitOfWork;
        _proveedorTiempo = proveedorTiempo;
        _validator = validator;
    }

    public async Task HandleAsync(CancelarAprobadaCommand comando, CancellationToken cancellationToken = default)
    {
        var fechaActual = _proveedorTiempo.ObtenerFechaActualCorporativa();

        var validacion = await _validator.ValidateAsync(comando, cancellationToken);
        if (!validacion.IsValid)
        {
            throw new ValidationException(validacion.Errors);
        }

        var solicitud = await _solicitudes.ObtenerPorIdAsync(comando.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException();

        solicitud.CancelarAprobada(fechaActual);

        var saldo = await _saldos.ObtenerPorEmpleadoIdAsync(solicitud.EmpleadoId, cancellationToken);
        if (saldo is not null)
        {
            saldo.RestaurarSaldo(solicitud.DiasRequeridos, fechaActual);
            await _saldos.ActualizarAsync(saldo, cancellationToken);
        }

        await _solicitudes.ActualizarAsync(solicitud, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}