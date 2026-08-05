using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vacations.Application.Saldos.Commands;

namespace Vacations.Infrastructure.Background;

/// <summary>
/// Background service que ejecuta la acumulación mensual de saldo (CU-01).
/// Se ejecuta el día 1 de cada mes a las 02:00 AM (configurable).
/// </summary>
public sealed class ServicioAcumuloMensual : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ServicioAcumuloMensual> _logger;
    private readonly TimeSpan _horaEjecucion = TimeSpan.FromHours(2); // 02:00 AM
    private readonly int _diaEjecucion = 1; // Día 1 de cada mes

    public ServicioAcumuloMensual(IServiceScopeFactory scopeFactory, ILogger<ServicioAcumuloMensual> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de acumulación mensual iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var ahora = DateTimeOffset.UtcNow;
                var proximaEjecucion = CalcularProximaEjecucion(ahora);
                var delay = proximaEjecucion - ahora;

                _logger.LogInformation("Próxima acumulación mensual programada para: {ProximaEjecucion}", proximaEjecucion);

                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await EjecutarAcumuloAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en servicio de acumulación mensual");
                // Esperar 1 hora antes de reintentar en caso de error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private DateTimeOffset CalcularProximaEjecucion(DateTimeOffset ahora)
    {
        var zonaCorporativa = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
        var ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(ahora.UtcDateTime, zonaCorporativa);

        var proximaEjecucionLocal = new DateTime(ahoraLocal.Year, ahoraLocal.Month, _diaEjecucion)
            .Add(_horaEjecucion);

        // Si ya pasó hoy, programar para el mes siguiente
        if (proximaEjecucionLocal <= ahoraLocal)
        {
            proximaEjecucionLocal = proximaEjecucionLocal.AddMonths(1);
        }

        return TimeZoneInfo.ConvertTimeToUtc(proximaEjecucionLocal, zonaCorporativa);
    }

    private async Task EjecutarAcumuloAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Ejecutando acumulación mensual de saldo");

        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<AcumularSaldoMensualCommandHandler>();

        try
        {
            await handler.HandleAsync(new AcumularSaldoMensualCommand(), cancellationToken);
            _logger.LogInformation("Acumulación mensual completada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la acumulación mensual");
            throw;
        }
    }
}