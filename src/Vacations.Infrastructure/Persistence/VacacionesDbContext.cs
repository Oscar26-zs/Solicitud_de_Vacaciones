using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Entities;
using Vacations.Infrastructure.Identity;

namespace Vacations.Infrastructure.Persistence;

/// <summary>
/// DbContext de la aplicación: entidades de dominio + Identity (constitution §5).
/// </summary>
public sealed class VacacionesDbContext : IdentityDbContext<UsuarioAplicacion, IdentityRole<Guid>, Guid>
{
    public DbSet<Empleado> Empleados => Set<Empleado>();

    public DbSet<SaldoEmpleado> SaldosEmpleados => Set<SaldoEmpleado>();

    public DbSet<SolicitudVacaciones> SolicitudesVacaciones => Set<SolicitudVacaciones>();

    public DbSet<HistorialSolicitud> HistorialSolicitudes => Set<HistorialSolicitud>();

    public VacacionesDbContext(DbContextOptions<VacacionesDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}