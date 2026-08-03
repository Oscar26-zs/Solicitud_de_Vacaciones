namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Comando para editar una solicitud en estado Pending (CU-06).</summary>
public sealed record EditarSolicitudCommand
{
    public Guid SolicitudId { get; init; }

    public Guid EmpleadoId { get; init; }

    public DateTime FechaInicio { get; init; }

    public DateTime FechaFin { get; init; }

    public string Motivo { get; init; } = default!;
}