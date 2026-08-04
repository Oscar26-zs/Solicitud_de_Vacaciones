using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vacations.Application.Saldos.Commands;
using Vacations.Application.Saldos.Queries;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;

namespace Vacations.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CrearSolicitudCommandHandler>();
        services.AddScoped<EditarSolicitudCommandHandler>();
        services.AddScoped<CancelarSolicitudCommandHandler>();
        services.AddScoped<AprobarSolicitudCommandHandler>();
        services.AddScoped<RechazarSolicitudCommandHandler>();
        services.AddScoped<CancelarAprobadaCommandHandler>();

        services.AddScoped<ObtenerMisSolicitudesQueryHandler>();
        services.AddScoped<ObtenerSolicitudDetalleQueryHandler>();
        services.AddScoped<ObtenerBandejaAprobadorQueryHandler>();
        services.AddScoped<ObtenerSolicitudesRRHHQueryHandler>();

        services.AddScoped<ObtenerSaldoQueryHandler>();
        services.AddScoped<AcumularSaldoMensualCommandHandler>();

        services.AddValidatorsFromAssemblyContaining<CrearSolicitudCommandValidator>();

        return services;
    }
}
