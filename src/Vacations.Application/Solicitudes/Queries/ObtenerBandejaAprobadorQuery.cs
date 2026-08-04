namespace Vacations.Application.Solicitudes.Queries;

public sealed record ObtenerBandejaAprobadorQuery(
    Guid AprobadorId,
    string? FiltroEmpleado = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    int Page = 1,
    int PageSize = 10);
