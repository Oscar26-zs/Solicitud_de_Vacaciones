using Vacations.Application.Solicitudes.Queries;

namespace Vacations.Web.ViewModels;

public class DetalleSolicitudViewModel
{
    public SolicitudDetalleDto Solicitud { get; set; } = null!;
    public bool PuedeEditar { get; set; }
    public bool PuedeCancelar { get; set; }
    public bool EsAprobador { get; set; }
    public bool PuedeCancelarAprobada { get; set; }
}
