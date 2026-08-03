using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

/// <summary>Query para listar las solicitudes del empleado autenticado (CU-05).</summary>
public sealed record ObtenerMisSolicitudesQuery
{
    public Guid EmpleadoId { get; init; }

    public EstadoSolicitud? Estado { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}