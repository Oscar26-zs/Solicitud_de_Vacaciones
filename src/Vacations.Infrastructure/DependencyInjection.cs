using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vacations.Domain.Abstractions;
using Vacations.Infrastructure.BackgroundServices;
using Vacations.Infrastructure.Identity;
using Vacations.Infrastructure.Persistence;
using Vacations.Infrastructure.Persistence.Repositories;

namespace Vacations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<VacacionesDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        services.AddIdentity<UsuarioAplicacion, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<VacacionesDbContext>()
        .AddDefaultTokenProviders()
        .AddClaimsPrincipalFactory<UsuarioClaimsPrincipalFactory>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Cuenta/Login";
            options.LogoutPath = "/Cuenta/Logout";
            options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
        });

        services.AddScoped<IRepositorioSolicitudVacaciones, RepositorioSolicitudVacaciones>();
        services.AddScoped<IRepositorioSaldoEmpleado, RepositorioSaldoEmpleado>();
        services.AddScoped<IRepositorioEmpleado, RepositorioEmpleado>();
        services.AddScoped<IRepositorioHistorialSolicitud, RepositorioHistorialSolicitud>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<VacacionesDbContext>());

        services.AddSingleton(TimeProvider.System);

        services.AddHostedService<ServicioExpiracionAutomatica>();

        return services;
    }
}
