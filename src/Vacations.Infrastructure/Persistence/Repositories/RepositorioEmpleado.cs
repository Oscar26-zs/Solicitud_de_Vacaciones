using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Infrastructure.Persistence;

namespace Vacations.Infrastructure.Persistence.Repositories;

public sealed class RepositorioEmpleado : IRepositorioEmpleado
{
    private readonly VacacionesDbContext _context;

    public RepositorioEmpleado(VacacionesDbContext context)
    {
        _context = context;
    }

    public async Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Empleados.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Empleado>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
        => await _context.Empleados
            .Where(e => e.EstaActivo)
            .OrderBy(e => e.NombreCompleto)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteConEmailAsync(string email, CancellationToken cancellationToken = default)
        => _context.Empleados.AnyAsync(e => e.Email == email, cancellationToken);
}