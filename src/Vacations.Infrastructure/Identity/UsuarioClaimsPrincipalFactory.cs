using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Vacations.Infrastructure.Identity;

public class UsuarioClaimsPrincipalFactory : UserClaimsPrincipalFactory<UsuarioAplicacion, IdentityRole<Guid>>
{
    public UsuarioClaimsPrincipalFactory(
        UserManager<UsuarioAplicacion> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(UsuarioAplicacion user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim("EmpleadoId", user.EmpleadoId.ToString()));
        return identity;
    }
}
