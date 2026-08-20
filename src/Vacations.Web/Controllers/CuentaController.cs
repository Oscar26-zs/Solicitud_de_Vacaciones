using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Vacations.Infrastructure.Identity;
using Vacations.Web.Authorization;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

public class CuentaController : Controller
{
    private readonly SignInManager<UsuarioAplicacion> _signInManager;
    private readonly UserManager<UsuarioAplicacion> _userManager;

    public CuentaController(
        SignInManager<UsuarioAplicacion> signInManager,
        UserManager<UsuarioAplicacion> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRolePage();
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToRolePage();
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "La cuenta ha sido bloqueada temporalmente. Intente de nuevo más tarde.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }

    private IActionResult RedirectToRolePage()
    {
        if (User.IsInRole(Roles.Aprobador))
        {
            return RedirectToAction("Index", "BandejaAprobador");
        }

        if (User.IsInRole(Roles.RRHH))
        {
            return RedirectToAction("Solicitudes", "RRHH");
        }

        return RedirectToAction("Index", "SolicitudVacaciones");
    }
}
