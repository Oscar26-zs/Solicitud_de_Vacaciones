using Microsoft.AspNetCore.Identity;
using Vacations.Domain.Entities;

namespace Vacations.Infrastructure.Identity;

/// <summary>
/// Usuario de ASP.NET Core Identity que se relaciona con la entidad de dominio
/// <see cref="Empleado"/> mediante <see cref="EmpleadoId"/>.
/// </summary>
public class UsuarioAplicacion : IdentityUser<Guid>
{
    public Guid? EmpleadoId { get; set; }

    public Empleado? Empleado { get; set; }
}