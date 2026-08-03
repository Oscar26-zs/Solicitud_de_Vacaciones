using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vacations.Infrastructure.Identity;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

/// <summary>Autenticación: login/logout (TASK-052).</summary>
public sealed class CuentaController : Controller
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
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resultado = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (!resultado.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
            return View(model);
        }

        return RedirigirSegunRol(returnUrl);
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
        => View();

    private IActionResult RedirigirSegunRol(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        var usuario = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
        if (usuario is not null
            && _userManager.IsInRoleAsync(usuario, "Aprobador").GetAwaiter().GetResult())
        {
            return RedirectToAction("Index", "BandejaAprobador");
        }

        return RedirectToAction("Index", "SolicitudVacaciones");
    }
}