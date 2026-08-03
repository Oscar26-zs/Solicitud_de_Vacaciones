using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

/// <summary>Query de solo lectura para RRHH sobre solicitudes de cualquier empleado (CU-18).</summary>
public sealed record ObtenerHistorialRRHHQuery
{
    public EstadoSolicitud? Estado { get; init; }

    public Guid? EmpleadoId { get; init; }

    public DateTime? FechaInicio { get; init; }

    public DateTime? FechaFin { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}