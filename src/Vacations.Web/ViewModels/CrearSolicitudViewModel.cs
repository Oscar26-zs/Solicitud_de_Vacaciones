using System.ComponentModel.DataAnnotations;

namespace Vacations.Web.ViewModels;

public class CrearSolicitudViewModel
{
    [Required(ErrorMessage = "La fecha de inicio es requerida.")]
    [Display(Name = "Fecha de inicio")]
    [DataType(DataType.Date)]
    public DateOnly FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es requerida.")]
    [Display(Name = "Fecha de fin")]
    [DataType(DataType.Date)]
    public DateOnly FechaFin { get; set; }

    [MaxLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
    [Display(Name = "Comentario")]
    public string? Comentario { get; set; }

    public int? DiasCalculados { get; set; }
    public int SaldoDisponible { get; set; }
}
