using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vacations.Domain.Abstractions;
using Vacations.Infrastructure.Background;
using Vacations.Infrastructure.Identity;
using Vacations.Infrastructure.Persistence;
using Vacations.Infrastructure.Persistence.Interceptors;
using Vacations.Infrastructure.Persistence.Repositories;
using Vacations.Infrastructure.Time;

namespace Vacations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("VacacionesDb")
            ?? throw new InvalidOperationException("La cadena de conexión 'VacacionesDb' no está configurada.");

        services.AddDbContext<VacacionesDbContext>((sp, options) =>
            options
                .UseSqlServer(connectionString)
                .AddInterceptors(sp.GetRequiredService<InterceptorAuditoria>()));

        services.AddScoped<InterceptorAuditoria>();

        services
            .AddIdentityCore<UsuarioAplicacion>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<VacacionesDbContext>();

        services.AddScoped<IRepositorioSolicitudVacaciones, RepositorioSolicitudVacaciones>();
        services.AddScoped<IRepositorioSaldoEmpleado, RepositorioSaldoEmpleado>();
        services.AddScoped<IRepositorioEmpleado, RepositorioEmpleado>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var proveedorTiempo = new ProveedorTiempoSistema(TimeProvider.System);
        services.AddSingleton<TimeProvider>(proveedorTiempo);
        services.AddSingleton<IProveedorTiempoCorporativo>(proveedorTiempo);

        services.AddHostedService<ServicioExpiracionAutomatica>();
        services.AddHostedService<ServicioAcumuloMensual>();

        return services;
    }
}