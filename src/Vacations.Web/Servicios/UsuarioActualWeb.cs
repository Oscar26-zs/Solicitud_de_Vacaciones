using Microsoft.AspNetCore.Identity;
using Vacations.Application.Abstractions;
using Vacations.Infrastructure.Identity;

namespace Vacations.Web.Servicios;

/// <summary>
/// Implementación de <see cref="IUsuarioActual"/> que lee la identidad del
/// HttpContext y resuelve el empleado vinculado (TASK-043/044).
/// </summary>
public interface IUsuarioActualWeb : IUsuarioActual
{
}

public sealed class UsuarioActualWeb : IUsuarioActualWeb
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<UsuarioAplicacion> _userManager;

    public UsuarioActualWeb(
        IHttpContextAccessor httpContextAccessor,
        UserManager<UsuarioAplicacion> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public Guid? UsuarioId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public Guid? EmpleadoId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User is null)
            {
                return null;
            }

            // En tests se inyecta el claim de empleado directamente.
            var claimEmpleado = httpContext.User.FindFirst("EmpleadoId")?.Value;
            if (Guid.TryParse(claimEmpleado, out var empleadoId))
            {
                return empleadoId;
            }

            // En producción se resuelve a través del store de Identity.
            var claimUsuario = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(claimUsuario, out var usuarioId))
            {
                return null;
            }

            var usuario = _userManager.FindByIdAsync(usuarioId.ToString()).GetAwaiter().GetResult();
            return usuario?.EmpleadoId;
        }
    }

public string? Email
        => _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    public IReadOnlyCollection<string> Roles
        => (_httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray()) ?? Array.Empty<string>();
}