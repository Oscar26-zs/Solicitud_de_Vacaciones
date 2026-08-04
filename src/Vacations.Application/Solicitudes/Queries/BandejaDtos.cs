using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

public sealed record BandejaSolicitudDto(
    Guid Id,
    Guid EmpleadoId,
    string EmpleadoNombre,
    string EmpleadoEmail,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int DiasRequeridos,
    string Motivo,
    DateTime CreadoEn,
    int SaldoDisponibleEmpleado);

public sealed record BandejaAprobadorResultado(
    IReadOnlyList<BandejaSolicitudDto> Solicitudes,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
