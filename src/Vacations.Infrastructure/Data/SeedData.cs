using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Entities;
using Vacations.Infrastructure.Identity;
using Vacations.Infrastructure.Persistence;

namespace Vacations.Infrastructure.Data;

public static class SeedData
{
    public static async Task ApplyMigrationsAsync(VacacionesDbContext dbContext)
    {
        await dbContext.Database.MigrateAsync();
    }

    public static async Task InitializeAsync(
        VacacionesDbContext dbContext,
        UserManager<UsuarioAplicacion> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        await SeedRolesAsync(roleManager);
        await SeedUsersAndEmployeesAsync(dbContext, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles = ["Empleado", "Aprobador", "RRHH"];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
        }
    }

    private static async Task SeedUsersAndEmployeesAsync(
        VacacionesDbContext dbContext,
        UserManager<UsuarioAplicacion> userManager)
    {
        var fechaActual = DateTime.UtcNow;

        // Empleado 1 - Rol Empleado
        var empleado1Email = "empleado@example.com";
        if (await userManager.FindByEmailAsync(empleado1Email) == null)
        {
            var empleado1 = Empleado.Crear(empleado1Email, "Juan Pérez", DateOnly.FromDateTime(fechaActual.AddYears(-2)));
            dbContext.Empleados.Add(empleado1);

            var usuario1 = new UsuarioAplicacion
            {
                Id = Guid.NewGuid(),
                UserName = empleado1Email,
                Email = empleado1Email,
                EmpleadoId = empleado1.Id,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var resultado1 = await userManager.CreateAsync(usuario1, "Empleado123!");
            if (!resultado1.Succeeded) throw new InvalidOperationException(string.Join("; ", resultado1.Errors.Select(e => e.Description)));
            await userManager.AddToRoleAsync(usuario1, "Empleado");

            var saldo1 = SaldoEmpleado.Crear(empleado1.Id, fechaActual);
            saldo1.AcumularDias(24, fechaActual); // 2 años = 24 días
            dbContext.SaldosEmpleado.Add(saldo1);
        }

        // Empleado 2 - Rol Aprobador
        var aprobadorEmail = "aprobador@example.com";
        if (await userManager.FindByEmailAsync(aprobadorEmail) == null)
        {
            var empleado2 = Empleado.Crear(aprobadorEmail, "María García", DateOnly.FromDateTime(fechaActual.AddYears(-3)));
            dbContext.Empleados.Add(empleado2);

            var usuario2 = new UsuarioAplicacion
            {
                Id = Guid.NewGuid(),
                UserName = aprobadorEmail,
                Email = aprobadorEmail,
                EmpleadoId = empleado2.Id,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var resultado2 = await userManager.CreateAsync(usuario2, "Aprobador123!");
            if (!resultado2.Succeeded) throw new InvalidOperationException(string.Join("; ", resultado2.Errors.Select(e => e.Description)));
            await userManager.AddToRolesAsync(usuario2, ["Empleado", "Aprobador"]);

            var saldo2 = SaldoEmpleado.Crear(empleado2.Id, fechaActual);
            saldo2.AcumularDias(36, fechaActual); // 3 años = 36 días
            dbContext.SaldosEmpleado.Add(saldo2);
        }

        // Empleado 4 - Rol Aprobador
        var aprobador2Email = "aprobador2@example.com";
        if (await userManager.FindByEmailAsync(aprobador2Email) == null)
        {
            var empleado4 = Empleado.Crear(aprobador2Email, "Carlos Rodríguez", DateOnly.FromDateTime(fechaActual.AddYears(-4)));
            dbContext.Empleados.Add(empleado4);

            var usuario4 = new UsuarioAplicacion
            {
                Id = Guid.NewGuid(),
                UserName = aprobador2Email,
                Email = aprobador2Email,
                EmpleadoId = empleado4.Id,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var resultado4 = await userManager.CreateAsync(usuario4, "Aprobador123!");
            if (!resultado4.Succeeded) throw new InvalidOperationException(string.Join("; ", resultado4.Errors.Select(e => e.Description)));
            await userManager.AddToRolesAsync(usuario4, ["Empleado", "Aprobador"]);

            var saldo4 = SaldoEmpleado.Crear(empleado4.Id, fechaActual);
            saldo4.AcumularDias(48, fechaActual); // 4 años = 48 días
            dbContext.SaldosEmpleado.Add(saldo4);
        }

        // Empleado 3 - Rol RRHH
        var rrhhEmail = "rrhh@example.com";
        if (await userManager.FindByEmailAsync(rrhhEmail) == null)
        {
            var empleado3 = Empleado.Crear(rrhhEmail, "Ana Martínez", DateOnly.FromDateTime(fechaActual.AddYears(-5)));
            dbContext.Empleados.Add(empleado3);

            var usuario3 = new UsuarioAplicacion
            {
                Id = Guid.NewGuid(),
                UserName = rrhhEmail,
                Email = rrhhEmail,
                EmpleadoId = empleado3.Id,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var resultado3 = await userManager.CreateAsync(usuario3, "Rrhh123!");
            if (!resultado3.Succeeded) throw new InvalidOperationException(string.Join("; ", resultado3.Errors.Select(e => e.Description)));
            await userManager.AddToRolesAsync(usuario3, ["Empleado", "RRHH"]);

            var saldo3 = SaldoEmpleado.Crear(empleado3.Id, fechaActual);
            saldo3.AcumularDias(60, fechaActual); // 5 años = 60 días
            dbContext.SaldosEmpleado.Add(saldo3);
        }

        await dbContext.SaveChangesAsync();
    }
}
