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

    [MaxLength(1000, ErrorMessage = "El motivo no puede exceder los 1000 caracteres.")]
    [Display(Name = "Motivo (opcional)")]
    public string? Motivo { get; set; }

    public int? DiasCalculados { get; set; }
    public int SaldoDisponible { get; set; }
}
