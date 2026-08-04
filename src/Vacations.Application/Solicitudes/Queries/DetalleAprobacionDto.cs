using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

public sealed record DetalleAprobacionDto(
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
    DateTime CreadoEn,
    int SaldoDisponible,
    bool TraslapeAprobada,
    bool TraslapePendiente,
    IReadOnlyList<HistorialEventoDto> Historial);

public sealed record SolicitudTraslapadaDto(
    Guid Id,
    EstadoSolicitud Estado,
    DateOnly FechaInicio,
    DateOnly FechaFin);
