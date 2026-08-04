using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;

namespace Vacations.Infrastructure.Persistence.Repositories;

public class RepositorioEmpleado : IRepositorioEmpleado
{
    private readonly VacacionesDbContext _context;

    public RepositorioEmpleado(VacacionesDbContext context)
    {
        _context = context;
    }

    public async Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Empleados
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Empleado?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();
        return await _context.Empleados
            .FirstOrDefaultAsync(e => e.Email == emailNormalizado, cancellationToken);
    }

    public async Task<IReadOnlyList<Empleado>> ObtenerActivosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Empleados
            .Where(e => e.EstaActivo)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Empleado>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Empleados
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(Empleado empleado, CancellationToken cancellationToken = default)
    {
        await _context.Empleados.AddAsync(empleado, cancellationToken);
    }

    public void Actualizar(Empleado empleado)
    {
        _context.Empleados.Update(empleado);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Empleados.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
