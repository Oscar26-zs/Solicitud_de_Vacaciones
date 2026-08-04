using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

public sealed record SolicitudRRHHDto(
    Guid Id,
    Guid EmpleadoId,
    string EmpleadoNombre,
    string EmpleadoEmail,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int DiasRequeridos,
    EstadoSolicitud Estado,
    string Motivo,
    string? ComentarioAprobador,
    DateTime CreadoEn);

public sealed record ListaSolicitudesRRHHResultado(
    IReadOnlyList<SolicitudRRHHDto> Solicitudes,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
