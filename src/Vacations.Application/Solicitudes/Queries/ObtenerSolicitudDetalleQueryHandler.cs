using Vacations.Domain.Abstractions;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Queries;

public sealed class ObtenerSolicitudDetalleQueryHandler
{
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;
    private readonly IRepositorioHistorialSolicitud _repositorioHistorial;
    private readonly IRepositorioEmpleado _repositorioEmpleados;

    public ObtenerSolicitudDetalleQueryHandler(
        IRepositorioSolicitudVacaciones repositorioSolicitudes,
        IRepositorioHistorialSolicitud repositorioHistorial,
        IRepositorioEmpleado repositorioEmpleados)
    {
        _repositorioSolicitudes = repositorioSolicitudes;
        _repositorioHistorial = repositorioHistorial;
        _repositorioEmpleados = repositorioEmpleados;
    }

    public async Task<SolicitudDetalleDto> HandleAsync(
        ObtenerSolicitudDetalleQuery query,
        CancellationToken cancellationToken = default)
    {
        var solicitud = await _repositorioSolicitudes.ObtenerPorIdAsync(query.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException(query.SolicitudId);

        var tieneAcceso = solicitud.EmpleadoId == query.UsuarioId || query.EsAprobador || query.EsRRHH;

        if (!tieneAcceso)
        {
            throw new AccesoNoAutorizadoException("No tiene permiso para ver esta solicitud.");
        }

        var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(solicitud.EmpleadoId, cancellationToken);
        var historial = await _repositorioHistorial.ObtenerPorSolicitudIdAsync(solicitud.Id, cancellationToken);

        var historialDtos = historial.Select(h => new HistorialEventoDto(
            h.TipoEvento,
            h.EstadoAnterior,
            h.EstadoNuevo,
            h.Actor,
            h.Timestamp,
            h.Comentario)).ToList();

        return new SolicitudDetalleDto(
            solicitud.Id,
            solicitud.EmpleadoId,
            empleado?.NombreCompleto ?? "Desconocido",
            solicitud.FechaInicio,
            solicitud.FechaFin,
            solicitud.DiasRequeridos,
            solicitud.Estado,
            solicitud.Motivo,
            solicitud.ComentarioAprobador,
            solicitud.AprobadoPor,
            solicitud.CreadoEn,
            solicitud.ActualizadoEn,
            historialDtos);
    }
}
