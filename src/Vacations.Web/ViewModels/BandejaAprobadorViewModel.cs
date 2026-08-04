using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;

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
    public EstadoSolicitud? FiltroEstado { get; set; }

    public int Pendientes { get; set; }
    public int Aprobadas { get; set; }
    public int Rechazadas { get; set; }
    public int Colaboradores { get; set; }
    public int DiasAprobados { get; set; }
}
