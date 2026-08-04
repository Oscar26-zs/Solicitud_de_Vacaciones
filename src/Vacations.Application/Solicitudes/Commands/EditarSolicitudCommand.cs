namespace Vacations.Application.Solicitudes.Commands;

public sealed record EditarSolicitudCommand(
    Guid SolicitudId,
    Guid EmpleadoId,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    string Motivo);
