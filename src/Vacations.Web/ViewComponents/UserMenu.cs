using Microsoft.AspNetCore.Mvc;

namespace Vacations.Web.ViewComponents;

public sealed class UserMenu : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var rol = User.IsInRole("RRHH") ? "RRHH"
            : User.IsInRole("Aprobador") ? "Aprobador"
            : "Empleado";

        var email = User.Identity?.Name ?? string.Empty;

        var model = new UserMenuModel(email, rol);
        return View(model);
    }
}

public sealed record UserMenuModel(string Email, string Rol);