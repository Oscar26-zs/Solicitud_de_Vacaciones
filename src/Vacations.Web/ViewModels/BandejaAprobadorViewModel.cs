using Vacations.Application.Solicitudes.Queries;

namespace Vacations.Web.ViewModels;

public class BandejaAprobadorViewModel
{
    public IReadOnlyList<BandejaSolicitudDto> Solicitudes { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? FiltroEmpleado { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
}
