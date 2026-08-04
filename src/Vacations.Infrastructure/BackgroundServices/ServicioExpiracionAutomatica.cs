using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;

namespace Vacations.Infrastructure.BackgroundServices;

public class ServicioExpiracionAutomatica : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ServicioExpiracionAutomatica> _logger;
    private readonly TimeSpan _intervalo;

    private const string ActorSistema = "SISTEMA_AUTO_EXPIRACION";

    public ServicioExpiracionAutomatica(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<ServicioExpiracionAutomatica> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
        _intervalo = TimeSpan.FromHours(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de expiración automática iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarExpiracionesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar expiraciones automáticas");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task ProcesarExpiracionesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var repositorioSolicitudes = scope.ServiceProvider.GetRequiredService<IRepositorioSolicitudVacaciones>();
        var repositorioSaldos = scope.ServiceProvider.GetRequiredService<IRepositorioSaldoEmpleado>();
        var repositorioHistorial = scope.ServiceProvider.GetRequiredService<IRepositorioHistorialSolicitud>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var fechaActual = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        var ahora = _timeProvider.GetUtcNow().DateTime;

        var solicitudesExpirables = await repositorioSolicitudes
            .ObtenerPendientesExpirablesAsync(fechaActual, cancellationToken);

        if (solicitudesExpirables.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Procesando {Count} solicitudes para expiración", solicitudesExpirables.Count);

        foreach (var solicitud in solicitudesExpirables)
        {
            try
            {
                var estadoAnterior = solicitud.Estado;
                solicitud.Expirar(ahora);

                var saldo = await repositorioSaldos.ObtenerPorEmpleadoIdAsync(solicitud.EmpleadoId, cancellationToken);
                if (saldo != null)
                {
                    saldo.LiberarSaldoPendiente(solicitud.DiasRequeridos, ahora);
                    repositorioSaldos.Actualizar(saldo);
                }

                var historial = HistorialSolicitud.CrearParaCambioEstado(
                    solicitud.Id,
                    estadoAnterior,
                    EstadoSolicitud.Expired,
                    ActorSistema,
                    ahora);

                await repositorioHistorial.AgregarAsync(historial, cancellationToken);
                repositorioSolicitudes.Actualizar(solicitud);

                _logger.LogInformation(
                    "Solicitud {SolicitudId} expirada automáticamente",
                    solicitud.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al expirar solicitud {SolicitudId}",
                    solicitud.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
