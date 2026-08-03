namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Comando para que un aprobador apruebe una solicitud (CU-11).</summary>
public sealed record AprobarSolicitudCommand
{
    public Guid SolicitudId { get; init; }

    /// <summary>Id del empleado aprobador (rol Aprobador).</summary>
    public Guid AprobadorEmpleadoId { get; init; }
}