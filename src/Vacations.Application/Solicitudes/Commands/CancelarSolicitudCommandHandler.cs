using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Commands;

public sealed class CancelarSolicitudCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;
    private readonly IRepositorioSaldoEmpleado _repositorioSaldos;
    private readonly IRepositorioHistorialSolicitud _repositorioHistorial;
    private readonly IRepositorioEmpleado _repositorioEmpleados;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CancelarSolicitudCommandHandler(
        IRepositorioSolicitudVacaciones repositorioSolicitudes,
        IRepositorioSaldoEmpleado repositorioSaldos,
        IRepositorioHistorialSolicitud repositorioHistorial,
        IRepositorioEmpleado repositorioEmpleados,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _repositorioSolicitudes = repositorioSolicitudes;
        _repositorioSaldos = repositorioSaldos;
        _repositorioHistorial = repositorioHistorial;
        _repositorioEmpleados = repositorioEmpleados;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(CancelarSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        var ahora = _timeProvider.GetUtcNow().DateTime;

        var solicitud = await _repositorioSolicitudes.ObtenerPorIdAsync(command.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException(command.SolicitudId);

        if (solicitud.EmpleadoId != command.EmpleadoId)
        {
            throw new AccesoNoAutorizadoException("No tiene permiso para cancelar esta solicitud.");
        }

        if (!solicitud.PuedeSerCanceladaPorEmpleado())
        {
            throw new CancelacionNoPermitidaException("Solo se pueden cancelar solicitudes en estado Pending.");
        }

        var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(command.EmpleadoId, cancellationToken)
            ?? throw new InvalidOperationException($"Empleado con Id '{command.EmpleadoId}' no encontrado.");

        var estadoAnterior = solicitud.Estado;
        solicitud.Cancelar(ahora);

        var saldo = await _repositorioSaldos.ObtenerPorEmpleadoIdAsync(command.EmpleadoId, cancellationToken)
            ?? throw new InvalidOperationException($"Saldo del empleado '{command.EmpleadoId}' no encontrado.");

        saldo.LiberarSaldoPendiente(solicitud.DiasRequeridos, ahora);

        var historial = HistorialSolicitud.CrearParaCambioEstado(
            solicitud.Id,
            estadoAnterior,
            EstadoSolicitud.Cancelled,
            empleado.Email,
            ahora);

        _repositorioSolicitudes.Actualizar(solicitud);
        _repositorioSaldos.Actualizar(saldo);
        await _repositorioHistorial.AgregarAsync(historial, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
