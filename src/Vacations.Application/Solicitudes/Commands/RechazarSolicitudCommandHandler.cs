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
    private readonly TimeProvider _timeProvider;

    public RechazarSolicitudCommandHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioSaldoEmpleado saldos,
        IRepositorioEmpleado empleados,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _solicitudes = solicitudes;
        _saldos = saldos;
        _empleados = empleados;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(RechazarSolicitudCommand comando, CancellationToken cancellationToken = default)
    {
        var fechaActual = _timeProvider.GetUtcNow().UtcDateTime.Date;

        if (string.IsNullOrWhiteSpace(comando.Comentario)
            || comando.Comentario.Trim().Length is < 1 or > SolicitudVacaciones.ComentarioMaxLength)
        {
            throw new ArgumentException($"El comentario es obligatorio y no puede exceder {SolicitudVacaciones.ComentarioMaxLength} caracteres.");
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