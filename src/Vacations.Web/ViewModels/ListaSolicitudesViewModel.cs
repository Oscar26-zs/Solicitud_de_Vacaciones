using Vacations.Application.Common;

namespace Vacations.Web.ViewModels;

/// <summary>ViewModel para listar las solicitudes del empleado con paginación (HU-02).</summary>
public sealed class ListaSolicitudesViewModel
{
    public PagedResult<SolicitudDto> PagedResult { get; set; } = default!;

    public string? EstadoFiltro { get; set; }
}