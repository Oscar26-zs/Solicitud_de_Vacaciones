using System.ComponentModel.DataAnnotations;

namespace Vacations.Web.ViewModels;

public class RechazarSolicitudViewModel
{
    public Guid SolicitudId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int DiasRequeridos { get; set; }

    [Required(ErrorMessage = "El comentario es obligatorio al rechazar una solicitud.")]
    [MinLength(1, ErrorMessage = "El comentario debe tener al menos 1 carácter.")]
    [MaxLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
    [Display(Name = "Comentario")]
    public string Comentario { get; set; } = string.Empty;
}
