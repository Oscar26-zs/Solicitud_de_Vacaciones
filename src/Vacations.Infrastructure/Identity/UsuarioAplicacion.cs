using Microsoft.AspNetCore.Identity;

namespace Vacations.Infrastructure.Identity;

public class UsuarioAplicacion : IdentityUser<Guid>
{
    public Guid EmpleadoId { get; set; }
}
