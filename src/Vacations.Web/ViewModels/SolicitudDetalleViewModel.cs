using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;

namespace Vacations.Web.ViewModels;

public class SolicitudDetalleViewModel
{
    public Guid Id { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int DiasHabiles { get; set; }
    public EstadoSolicitud Estado { get; set; }
    public string? Comentario { get; set; }
    public DateTime FechaCreacion { get; set; }
    public IReadOnlyList<HistorialEventoDto> Historial { get; set; } = [];
    public bool PuedeEditar { get; set; }
    public bool PuedeCancelar { get; set; }
    public bool EsAprobador { get; set; }
    public bool PuedeCancelarAprobada { get; set; }
}
