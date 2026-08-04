using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;

namespace Vacations.Infrastructure.Persistence.Repositories;

public class RepositorioHistorialSolicitud : IRepositorioHistorialSolicitud
{
    private readonly VacacionesDbContext _context;

    public RepositorioHistorialSolicitud(VacacionesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<HistorialSolicitud>> ObtenerPorSolicitudIdAsync(
        Guid solicitudId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.HistorialSolicitudes
            .Where(h => h.SolicitudId == solicitudId)
            .OrderBy(h => h.Timestamp)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(HistorialSolicitud historial, CancellationToken cancellationToken = default)
    {
        await _context.HistorialSolicitudes.AddAsync(historial, cancellationToken);
    }
}
