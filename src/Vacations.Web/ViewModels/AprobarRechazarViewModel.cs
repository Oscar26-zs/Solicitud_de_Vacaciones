using System.ComponentModel.DataAnnotations;
using Vacations.Application.Common;

namespace Vacations.Web.ViewModels;

/// <summary>ViewModel para aprobar o rechazar con comentario obligatorio en rechazo (HU-06).</summary>
public sealed class AprobarRechazarViewModel
{
    public Guid SolicitudId { get; set; }

    public SolicitudDto Solicitud { get; set; } = default!;

    /// <summary>Comentario obligatorio para rechazar (0..500).</summary>
    [StringLength(500, ErrorMessage = "Comentario no puede exceder 500 caracteres")]
    public string? Comentario { get; set; }

    public bool EsRechazo { get; set; }
}