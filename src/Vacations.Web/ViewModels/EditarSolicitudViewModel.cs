using System.ComponentModel.DataAnnotations;

namespace Vacations.Web.ViewModels;

/// <summary>ViewModel para editar una solicitud pendiente (HU-03, CU-06).</summary>
public sealed class EditarSolicitudViewModel
{
    public Guid SolicitudId { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; }

    [Required(ErrorMessage = "El motivo es obligatorio")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "El motivo debe tener entre 10 y 1000 caracteres")]
    public string Motivo { get; set; } = default!;
}