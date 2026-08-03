namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Comando para que un empleado cancele su solicitud Pending (CU-07).</summary>
public sealed record CancelarSolicitudCommand
{
    public Guid SolicitudId { get; init; }

    public Guid EmpleadoId { get; init; }
}