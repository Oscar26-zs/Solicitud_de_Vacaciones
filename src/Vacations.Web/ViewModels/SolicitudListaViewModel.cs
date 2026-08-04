using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;

namespace Vacations.Web.ViewModels;

public class SolicitudListaViewModel
{
    public IReadOnlyList<SolicitudResumenDto> Solicitudes { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public EstadoSolicitud? FiltroEstado { get; set; }
}
