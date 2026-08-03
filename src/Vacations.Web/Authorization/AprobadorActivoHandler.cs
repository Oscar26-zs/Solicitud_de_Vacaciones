using Microsoft.AspNetCore.Authorization;
using Vacations.Application.Abstractions;
using Vacations.Domain.Abstractions;

namespace Vacations.Web.Authorization;

/// <summary>
/// Requisito personalizado que verifica que el aprobador esté activo (EstaActivo)
/// consultando el repositorio de empleados (TASK-044).
/// </summary>
public sealed class RequisitoAprobadorActivo : IAuthorizationRequirement
{
}

public sealed class AprobadorActivoHandler : AuthorizationHandler<RequisitoAprobadorActivo>
{
    private readonly IRepositorioEmpleado _empleados;
    private readonly IUsuarioActual _usuario;

    public AprobadorActivoHandler(IRepositorioEmpleado empleados, IUsuarioActual usuario)
    {
        _empleados = empleados;
        _usuario = usuario;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequisitoAprobadorActivo requirement)
    {
        if (_usuario.EmpleadoId is Guid empleadoId)
        {
            var empleado = await _empleados.ObtenerPorIdAsync(empleadoId);
            if (empleado is not null && empleado.EstaActivo)
            {
                context.Succeed(requirement);
            }
        }
    }
}