using Vacations.Application.Saldos.Queries;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;

namespace Vacations.Web.ViewModels;

public class EmpleadoDashboardViewModel
{
    public SaldoDto? Saldo { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public IReadOnlyList<SolicitudResumenDto> Solicitudes { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public EstadoSolicitud? FiltroEstado { get; set; }
    public int Pendientes { get; set; }
    public int Aprobadas { get; set; }
    public int Rechazadas { get; set; }
    public int Canceladas { get; set; }

    public CrearSolicitudViewModel? CrearSolicitud { get; set; }
    public EditarSolicitudViewModel? EditarSolicitud { get; set; }
    public bool SheetAbierta { get; set; }
}
