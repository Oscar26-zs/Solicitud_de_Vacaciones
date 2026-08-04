using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Infrastructure.Identity;

namespace Vacations.Infrastructure.Persistence;

public class VacacionesDbContext : IdentityDbContext<UsuarioAplicacion, IdentityRole<Guid>, Guid>, IUnitOfWork
{
    private readonly TimeProvider _timeProvider;

    public VacacionesDbContext(DbContextOptions<VacacionesDbContext> options, TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider;
    }

    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<SaldoEmpleado> SaldosEmpleado => Set<SaldoEmpleado>();
    public DbSet<SolicitudVacaciones> SolicitudesVacaciones => Set<SolicitudVacaciones>();
    public DbSet<HistorialSolicitud> HistorialSolicitudes => Set<HistorialSolicitud>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(VacacionesDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
