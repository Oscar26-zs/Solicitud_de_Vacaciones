namespace Vacations.Application.Solicitudes.Commands;

public sealed record CancelarAprobadaCommand(
    Guid SolicitudId,
    Guid AprobadorId);
