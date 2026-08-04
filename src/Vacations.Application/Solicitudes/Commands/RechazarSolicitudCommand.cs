namespace Vacations.Application.Solicitudes.Commands;

public sealed record RechazarSolicitudCommand(
    Guid SolicitudId,
    Guid AprobadorId,
    string Comentario);
