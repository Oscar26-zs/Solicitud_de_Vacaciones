namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Comando para que un aprobador rechace una solicitud con comentario obligatorio (CU-12).</summary>
public sealed record RechazarSolicitudCommand
{
    public Guid SolicitudId { get; init; }

    public Guid AprobadorEmpleadoId { get; init; }

    public string Comentario { get; init; } = default!;
}