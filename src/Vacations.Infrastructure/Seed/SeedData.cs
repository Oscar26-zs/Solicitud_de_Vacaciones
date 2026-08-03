using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Infrastructure.Identity;
using Vacations.Infrastructure.Persistence;

namespace Vacations.Infrastructure.Seed;

/// <summary>
/// Siembra datos iniciales: roles y usuarios de demostración (DEV/seed).
/// Los empleados y saldos se crean en el primer arranque de la app de demostración.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VacacionesDbContext>();

        try
        {
            await context.Database.MigrateAsync();
        }
        catch (InvalidOperationException)
        {
            // El proveedor (p.ej. InMemory en tests) no soporta migraciones.
            await context.Database.EnsureCreatedAsync();
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UsuarioAplicacion>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var rol in new[] { nameof(RolUsuario.Empleado), nameof(RolUsuario.Aprobador), nameof(RolUsuario.RRHH) })
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(rol));
            }
        }

        if (!await context.Empleados.AnyAsync())
        {
            var empleados = new[]
            {
                Empleado.Crear("ana@empresa.com", "Ana López", DateTime.Now.AddYears(-3)),
                Empleado.Crear("luis@empresa.com", "Luis Martínez", DateTime.Now.AddYears(-2)),
                Empleado.Crear("carla@empresa.com", "Carla Ruiz", DateTime.Now.AddYears(-4)),
            };

            foreach (var empleado in empleados)
            {
                context.Empleados.Add(empleado);

                var saldo = SaldoEmpleado.Crear(empleado.Id, DateTime.Now);
                saldo.AcumularDias(20, DateTime.Now);
                context.SaldosEmpleados.Add(saldo);
            }

            await context.SaveChangesAsync();

            await SeedUsuarioAsync(userManager, logger, "empleado@empresa.com", "Empleado123?", "Empleado", empleados[0].Id);
            await SeedUsuarioAsync(userManager, logger, "luis@empresa.com", "Aprobador123?", "Aprobador", empleados[1].Id);
            await SeedUsuarioAsync(userManager, logger, "carla@empresa.com", "Rrhh123?", "RRHH", empleados[2].Id);
        }

        logger.LogInformation("Datos iniciales sembrados");
    }

    private static async Task SeedUsuarioAsync(
        UserManager<UsuarioAplicacion> userManager,
        ILogger logger,
        string email,
        string password,
        string rol,
        Guid? empleadoId)
    {
        var existe = await userManager.FindByEmailAsync(email);
        if (existe is not null)
        {
            return;
        }

        var usuario = new UsuarioAplicacion
        {
            UserName = email,
            Email = email,
            EmpleadoId = empleadoId,
            EmailConfirmed = true,
        };

        var resultado = await userManager.CreateAsync(usuario, password);
        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(usuario, rol);
        }
        else
        {
            var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
            logger.LogWarning("Falló creación de usuario {Email}: {Errores}", email, errores);
        }
    }
}