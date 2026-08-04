namespace Vacations.Application.Solicitudes.Commands;

public sealed record CancelarSolicitudCommand(
    Guid SolicitudId,
    Guid EmpleadoId);
