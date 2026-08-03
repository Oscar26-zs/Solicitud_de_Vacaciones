namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Comando para crear una nueva solicitud de vacaciones (CU-04).</summary>
public sealed record CrearSolicitudCommand
{
    public Guid EmpleadoId { get; init; }

    public DateTime FechaInicio { get; init; }

    public DateTime FechaFin { get; init; }

    public string Motivo { get; init; } = default!;
}

public sealed record CrearSolicitudResult(Guid SolicitudId);