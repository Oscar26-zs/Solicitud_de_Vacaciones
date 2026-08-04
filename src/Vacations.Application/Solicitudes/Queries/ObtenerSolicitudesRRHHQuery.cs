using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

public sealed record ObtenerSolicitudesRRHHQuery(
    Guid? EmpleadoId = null,
    EstadoSolicitud? Estado = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    int Page = 1,
    int PageSize = 10);
