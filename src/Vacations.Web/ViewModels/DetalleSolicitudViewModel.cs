using Vacations.Application.Common;

namespace Vacations.Web.ViewModels;

/// <summary>ViewModel de detalle de una solicitud con su historial de auditoría (HU-02).</summary>
public sealed class DetalleSolicitudViewModel
{
    public SolicitudDto Solicitud { get; set; } = default!;

    public IReadOnlyList<HistorialEventoDto> Historial { get; set; } = Array.Empty<HistorialEventoDto>();

    public bool EsDueno { get; set; }

    public bool EsAprobador { get; set; }
}