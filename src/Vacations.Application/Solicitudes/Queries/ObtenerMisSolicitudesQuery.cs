using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

public sealed record ObtenerMisSolicitudesQuery(
    Guid EmpleadoId,
    EstadoSolicitud? Estado = null,
    int Page = 1,
    int PageSize = 10);
