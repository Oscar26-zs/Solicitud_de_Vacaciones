using Vacations.Domain.Abstractions;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Queries;

public sealed class ObtenerDetalleAprobacionQueryHandler
{
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;
    private readonly IRepositorioHistorialSolicitud _repositorioHistorial;
    private readonly IRepositorioEmpleado _repositorioEmpleados;
    private readonly IRepositorioSaldoEmpleado _repositorioSaldos;

    public ObtenerDetalleAprobacionQueryHandler(
        IRepositorioSolicitudVacaciones repositorioSolicitudes,
        IRepositorioHistorialSolicitud repositorioHistorial,
        IRepositorioEmpleado repositorioEmpleados,
        IRepositorioSaldoEmpleado repositorioSaldos)
    {
        _repositorioSolicitudes = repositorioSolicitudes;
        _repositorioHistorial = repositorioHistorial;
        _repositorioEmpleados = repositorioEmpleados;
        _repositorioSaldos = repositorioSaldos;
    }

    public async Task<DetalleAprobacionDto> HandleAsync(
        ObtenerDetalleAprobacionQuery query,
        CancellationToken cancellationToken = default)
    {
        var solicitud = await _repositorioSolicitudes.ObtenerPorIdAsync(query.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException(query.SolicitudId);

        var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(solicitud.EmpleadoId, cancellationToken);
        var saldo = await _repositorioSaldos.ObtenerPorEmpleadoIdAsync(solicitud.EmpleadoId, cancellationToken);
        var historial = await _repositorioHistorial.ObtenerPorSolicitudIdAsync(solicitud.Id, cancellationToken);

        var traslapes = await _repositorioSolicitudes.ObtenerTraslapesAsync(
            solicitud.EmpleadoId,
            solicitud.FechaInicio,
            solicitud.FechaFin,
            solicitud.Id,
            cancellationToken);

        var historialDtos = historial.Select(h => new HistorialEventoDto(
            h.TipoEvento,
            h.EstadoAnterior,
            h.EstadoNuevo,
            h.Actor,
            h.Timestamp,
            h.Comentario)).ToList();

        return new DetalleAprobacionDto(
            solicitud.Id,
            solicitud.EmpleadoId,
            empleado?.NombreCompleto ?? "Desconocido",
            empleado?.Email ?? "Desconocido",
            solicitud.FechaInicio,
            solicitud.FechaFin,
            solicitud.DiasRequeridos,
            solicitud.Estado,
            solicitud.Motivo,
            solicitud.ComentarioAprobador,
            solicitud.CreadoEn,
            saldo?.SaldoDisponible ?? 0,
            traslapes.Any(t => t.Estado == Domain.Enums.EstadoSolicitud.Approved),
            traslapes.Any(t => t.Estado == Domain.Enums.EstadoSolicitud.Pending),
            historialDtos);
    }
}
