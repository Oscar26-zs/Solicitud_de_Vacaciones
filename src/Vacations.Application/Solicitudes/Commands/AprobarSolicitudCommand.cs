namespace Vacations.Application.Solicitudes.Commands;

public sealed record AprobarSolicitudCommand(
    Guid SolicitudId,
    Guid AprobadorId);
