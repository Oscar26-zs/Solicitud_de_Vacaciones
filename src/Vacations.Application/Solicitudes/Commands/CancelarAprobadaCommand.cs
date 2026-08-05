namespace Vacations.Application.Solicitudes.Commands;

public sealed record CancelarAprobadaCommand(
    Guid SolicitudId,
    Guid AprobadorId,
    string Motivo = "Cancelación de solicitud aprobada");
