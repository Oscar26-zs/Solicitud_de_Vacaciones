namespace Vacations.Application.Solicitudes.Commands;

public sealed record CrearSolicitudCommand(
    Guid EmpleadoId,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    string Motivo);
