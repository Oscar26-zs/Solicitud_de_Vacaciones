using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Commands;

public sealed class CancelarAprobadaCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;
    private readonly IRepositorioSaldoEmpleado _repositorioSaldos;
    private readonly IRepositorioHistorialSolicitud _repositorioHistorial;
    private readonly IRepositorioEmpleado _repositorioEmpleados;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CancelarAprobadaCommandHandler(
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

    public async Task HandleAsync(CancelarAprobadaCommand command, CancellationToken cancellationToken = default)
    {
        var ahora = _timeProvider.GetUtcNow().DateTime;
        var fechaActual = DateOnly.FromDateTime(ahora);

        var solicitud = await _repositorioSolicitudes.ObtenerPorIdAsync(command.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException(command.SolicitudId);

        var aprobador = await _repositorioEmpleados.ObtenerPorIdAsync(command.AprobadorId, cancellationToken)
            ?? throw new InvalidOperationException($"Aprobador con Id '{command.AprobadorId}' no encontrado.");

        if (!aprobador.EstaActivo)
        {
            throw new AprobadorInactivoException();
        }

        var estadoAnterior = solicitud.Estado;
        solicitud.CancelarAprobada(command.AprobadorId, fechaActual, ahora);

        var saldo = await _repositorioSaldos.ObtenerPorEmpleadoIdAsync(solicitud.EmpleadoId, cancellationToken)
            ?? throw new InvalidOperationException($"Saldo del empleado '{solicitud.EmpleadoId}' no encontrado.");

        saldo.RestaurarSaldo(solicitud.DiasRequeridos, ahora);

        var historial = HistorialSolicitud.CrearParaCambioEstado(
            solicitud.Id,
            estadoAnterior,
            EstadoSolicitud.Cancelled,
            aprobador.Email,
            ahora,
            command.Motivo);

        _repositorioSolicitudes.Actualizar(solicitud);
        _repositorioSaldos.Actualizar(saldo);
        await _repositorioHistorial.AgregarAsync(historial, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
