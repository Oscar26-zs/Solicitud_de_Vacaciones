using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;

namespace Vacations.Infrastructure.Background;

/// <summary>
/// Servicio en segundo plano que expira solicitudes Pending cuya fecha de inicio
/// ya quedó atrás (RF-040, RN-05). Ejecuta de forma idempotente mediante la
/// fecha actual inyectada y libera el saldo pendiente asociado.
/// </summary>
public sealed class ServicioExpiracionAutomatica : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ServicioExpiracionAutomatica> _logger;
    private readonly TimeSpan _intervalo;
    private readonly TimeProvider _timeProvider;

    public ServicioExpiracionAutomatica(
        IServiceProvider services,
        ILogger<ServicioExpiracionAutomatica> logger,
        TimeProvider timeProvider,
        TimeSpan? intervalo = null)
    {
        _services = services;
        _logger = logger;
        _timeProvider = timeProvider;
        _intervalo = intervalo ?? TimeSpan.FromHours(12);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpirarPendientesVencidasAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar expiración automática de solicitudes");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task ExpirarPendientesVencidasAsync(CancellationToken cancellationToken)
    {
        var hoy = _timeProvider.GetUtcNow().UtcDateTime.Date;

        await using var scope = _services.CreateAsyncScope();
        var repositorio = scope.ServiceProvider.GetRequiredService<IRepositorioSolicitudVacaciones>();
        var repositorioSaldo = scope.ServiceProvider.GetRequiredService<IRepositorioSaldoEmpleado>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pendientes = await repositorio.ObtenerPendientesAsync(cancellationToken);
        var aExpirar = pendientes.Where(s => s.FechaInicio < hoy).ToList();

        foreach (var solicitud in aExpirar)
        {
            var saldo = await repositorioSaldo.ObtenerPorEmpleadoIdAsync(solicitud.EmpleadoId, cancellationToken);
            if (saldo is not null)
            {
                saldo.LiberarSaldoPendiente(solicitud.DiasRequeridos, hoy);
                await repositorioSaldo.ActualizarAsync(saldo, cancellationToken);
            }

            solicitud.Expirar(hoy);
            await repositorio.ActualizarAsync(solicitud, cancellationToken);
        }

        if (aExpirar.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Expiradas {Cantidad} solicitudes vencidas", aExpirar.Count);
        }
    }
}