using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Infrastructure.Persistence;

namespace Vacations.Infrastructure.Persistence.Repositories;

public sealed class RepositorioSolicitudVacaciones : IRepositorioSolicitudVacaciones
{
    private readonly VacacionesDbContext _context;

    public RepositorioSolicitudVacaciones(VacacionesDbContext context)
    {
        _context = context;
    }

    public async Task<SolicitudVacaciones?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.SolicitudesVacaciones.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPorEmpleadoAsync(Guid empleadoId, CancellationToken cancellationToken = default)
        => await _context.SolicitudesVacaciones
            .Where(s => s.EmpleadoId == empleadoId)
            .OrderByDescending(s => s.CreadoEn)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPendientesAsync(CancellationToken cancellationToken = default)
        => await _context.SolicitudesVacaciones
            .Where(s => s.Estado == Domain.Enums.EstadoSolicitud.Pending)
            .OrderBy(s => s.FechaInicio)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SolicitudVacaciones>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
        => await _context.SolicitudesVacaciones
            .OrderByDescending(s => s.CreadoEn)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExisteTraslapeAsync(        Guid empleadoId,
        DateTime inicio,
        DateTime fin,
        Guid? excluirSolicitudId,
        CancellationToken cancellationToken = default)
    {
        var pendienteOHabilitadas = new[]
        {
            Domain.Enums.EstadoSolicitud.Pending,
            Domain.Enums.EstadoSolicitud.Approved,
        };

        return await _context.SolicitudesVacaciones
            .Where(s => s.EmpleadoId == empleadoId)
            .Where(s => pendienteOHabilitadas.Contains(s.Estado))
            .Where(s => !excluirSolicitudId.HasValue || s.Id != excluirSolicitudId.Value)
            .AnyAsync(s => s.FechaInicio <= fin && s.FechaFin >= inicio, cancellationToken);
    }

    public async Task AgregarAsync(SolicitudVacaciones solicitud, CancellationToken cancellationToken = default)
        => await _context.SolicitudesVacaciones.AddAsync(solicitud, cancellationToken);

    public async Task<IReadOnlyList<HistorialSolicitud>> ObtenerHistorialAsync(Guid solicitudId, CancellationToken cancellationToken = default)
        => await _context.HistorialSolicitudes
            .Where(h => h.SolicitudId == solicitudId)
            .OrderBy(h => h.Timestamp)
            .ToListAsync(cancellationToken);

    public Task ActualizarAsync(SolicitudVacaciones solicitud, CancellationToken cancellationToken = default)
    {
        _context.SolicitudesVacaciones.Update(solicitud);
        return Task.CompletedTask;
    }
}