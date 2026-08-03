using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;

namespace Vacations.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor de auditoría que registra en <see cref="HistorialSolicitud"/> los
/// eventos CREATED / STATUS_CHANGED / CANCELLED generados sobre las solicitudes
/// (CU-17, RF-032). Solo escritura de creación: nunca modifica entidades.
/// </summary>
public sealed class InterceptorAuditoria : SaveChangesInterceptor
{
    public const string ActorSistema = "Sistema";

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            RegistrarEventos(eventData.Context);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            RegistrarEventos(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private static void RegistrarEventos(DbContext context)
    {
        var timestamp = DateTime.UtcNow;
        var solicitudes = context.ChangeTracker.Entries<SolicitudVacaciones>().ToList();

        foreach (var entrada in solicitudes)
        {
            switch (entrada.State)
            {
                case EntityState.Added:
                    context.Add(HistorialSolicitud.Crear(
                        entrada.Entity.Id,
                        HistorialSolicitud.EventoConstante.Creado,
                        ActorSistema,
                        timestamp,
                        estadoNuevo: EstadoSolicitud.Pending));
                    break;

                case EntityState.Modified:
                    if (entrada.OriginalValues[nameof(SolicitudVacaciones.Estado)] is EstadoSolicitud estadoAnterior &&
                        entrada.Entity.Estado != estadoAnterior)
                    {
                        context.Add(HistorialSolicitud.Crear(
                            entrada.Entity.Id,
                            entrada.Entity.Estado == EstadoSolicitud.Cancelled
                                ? HistorialSolicitud.EventoConstante.Cancelado
                                : HistorialSolicitud.EventoConstante.EstadoCambiado,
                            ActorSistema,
                            timestamp,
                            estadoAnterior,
                            entrada.Entity.Estado,
                            comentario: entrada.Entity.ComentarioAprobador));
                    }
                    break;
            }
        }
    }
}