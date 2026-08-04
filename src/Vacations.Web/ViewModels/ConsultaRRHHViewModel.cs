using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;

namespace Vacations.Web.ViewModels;

public class ConsultaRRHHViewModel
{
    public IReadOnlyList<SolicitudRRHHDto> Solicitudes { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public Guid? FiltroEmpleadoId { get; set; }
    public EstadoSolicitud? FiltroEstado { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
}
