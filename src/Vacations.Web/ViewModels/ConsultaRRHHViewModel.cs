using Vacations.Application.Common;
using Vacations.Application.Solicitudes.Queries;

namespace Vacations.Web.ViewModels;

/// <summary>ViewModel de consultas de solo lectura para RRHH (HU-08, HU-09).</summary>
public sealed class ConsultaRRHHViewModel
{
    public PagedResult<SolicitudRRHHItem> PagedResult { get; set; } = default!;

    public FiltrosRRHHViewModel Filtros { get; set; } = new();
}

public sealed class FiltrosRRHHViewModel
{
    public string? Estado { get; set; }

    public Guid? EmpleadoId { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }
}