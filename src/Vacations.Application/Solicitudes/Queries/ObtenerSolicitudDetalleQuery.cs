namespace Vacations.Application.Solicitudes.Queries;

/// <summary>Query para obtener el detalle de una solicitud incluyendo historial (CU-05).</summary>
public sealed record ObtenerSolicitudDetalleQuery
{
    public Guid SolicitudId { get; init; }

    /// <summary>Empleado autenticado solicitando el detalle (para validar acceso).</summary>
    public Guid EmpleadoSolicitanteId { get; init; }
}