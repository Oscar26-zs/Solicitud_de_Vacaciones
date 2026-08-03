using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

/// <summary>Query para listar solicitudes Pending visibles para un aprobador (CU-10).</summary>
public sealed record ObtenerBandejaAprobadorQuery
{
    /// <summary>Id del empleado aprobador autenticado (para excluir sus propias solicitudes).</summary>
    public Guid AprobadorEmpleadoId { get; init; }

    public Guid? EmpleadoId { get; init; }

    public DateTime? FechaInicio { get; init; }

    public DateTime? FechaFin { get; init; }

    public int? Dias { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}