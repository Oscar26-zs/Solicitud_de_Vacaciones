namespace Vacations.Application.Solicitudes.Queries;

public sealed record ObtenerDetalleAprobacionQuery(
    Guid SolicitudId,
    Guid AprobadorId);
