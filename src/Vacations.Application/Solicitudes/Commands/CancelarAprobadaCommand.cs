namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Comando para que un aprobador cancele una solicitud ya aprobada (CU-14).</summary>
public sealed record CancelarAprobadaCommand
{
    public Guid SolicitudId { get; init; }

    public Guid AprobadorEmpleadoId { get; init; }
}