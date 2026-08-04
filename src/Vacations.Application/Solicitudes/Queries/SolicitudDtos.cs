using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

public sealed record SolicitudResumenDto(
    Guid Id,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int DiasRequeridos,
    EstadoSolicitud Estado,
    string Motivo,
    string? ComentarioAprobador,
    DateTime CreadoEn);

public sealed record ListaSolicitudesResultado(
    IReadOnlyList<SolicitudResumenDto> Solicitudes,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
