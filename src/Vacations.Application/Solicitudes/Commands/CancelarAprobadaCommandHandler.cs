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
    private readonly TimeProvider _timeProvider;

    public CancelarAprobadaCommandHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioSaldoEmpleado saldos,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _solicitudes = solicitudes;
        _saldos = saldos;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(CancelarAprobadaCommand comando, CancellationToken cancellationToken = default)
    {
        var fechaActual = _timeProvider.GetUtcNow().UtcDateTime.Date;

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