using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

public sealed record SolicitudDetalleDto(
    Guid Id,
    Guid EmpleadoId,
    string EmpleadoNombre,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int DiasRequeridos,
    EstadoSolicitud Estado,
    string Motivo,
    string? ComentarioAprobador,
    Guid? AprobadoPor,
    DateTime CreadoEn,
    DateTime ActualizadoEn,
    IReadOnlyList<HistorialEventoDto> Historial);

public sealed record HistorialEventoDto(
    TipoEvento TipoEvento,
    EstadoSolicitud? EstadoAnterior,
    EstadoSolicitud? EstadoNuevo,
    string Actor,
    DateTime Timestamp,
    string? Comentario);
