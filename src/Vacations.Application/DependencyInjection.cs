using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vacations.Application.Saldos.Commands;
using Vacations.Application.Saldos.Queries;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;

namespace Vacations.Application;

/// <summary>Registro de servicios de la capa Application (handlers y validadores).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Comandos de solicitud
        services.AddScoped<CrearSolicitudCommandHandler>();
        services.AddScoped<EditarSolicitudCommandHandler>();
        services.AddScoped<CancelarSolicitudCommandHandler>();
        services.AddScoped<AprobarSolicitudCommandHandler>();
        services.AddScoped<RechazarSolicitudCommandHandler>();
        services.AddScoped<CancelarAprobadaCommandHandler>();

        // Comandos de saldo
        services.AddScoped<AcumularSaldoMensualCommandHandler>();

        // Queries de solicitud
        services.AddScoped<ObtenerMisSolicitudesQueryHandler>();
        services.AddScoped<ObtenerSolicitudDetalleQueryHandler>();
        services.AddScoped<ObtenerBandejaAprobadorQueryHandler>();
        services.AddScoped<ObtenerHistorialRRHHQueryHandler>();

        // Queries de saldo
        services.AddScoped<ObtenerSaldoQueryHandler>();

        return services;
    }
}