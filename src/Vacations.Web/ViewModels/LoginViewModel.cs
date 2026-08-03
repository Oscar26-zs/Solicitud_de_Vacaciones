using System.ComponentModel.DataAnnotations;

namespace Vacations.Web.ViewModels;

/// <summary>ViewModel de login (Identity).</summary>
public sealed class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}