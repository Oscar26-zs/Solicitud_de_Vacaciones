using Microsoft.AspNetCore.Authorization;

namespace Vacations.Web.Authorization;

/// <summary>Definición de políticas de autorización basadas en roles (constitution §1, §8).</summary>
public static class PoliticasAutorizacion
{
    public const string RequiereEmpleado = nameof(RequiereEmpleado);
    public const string RequiereAprobador = nameof(RequiereAprobador);
    public const string RequiereRRHH = nameof(RequiereRRHH);
    public const string RequiereAprobadorActivo = nameof(RequiereAprobadorActivo);

    public static void Configurar(AuthorizationOptions options)
    {
        options.AddPolicy(RequiereEmpleado, policy =>
            policy.RequireRole("Empleado", "Aprobador", "RRHH"));

        options.AddPolicy(RequiereAprobador, policy =>
            policy.RequireRole("Aprobador"));

        options.AddPolicy(RequiereRRHH, policy =>
            policy.RequireRole("RRHH"));

        options.AddPolicy(RequiereAprobadorActivo, policy =>
            policy.RequireRole("Aprobador")
                  .AddRequirements(new RequisitoAprobadorActivo()));
    }
}