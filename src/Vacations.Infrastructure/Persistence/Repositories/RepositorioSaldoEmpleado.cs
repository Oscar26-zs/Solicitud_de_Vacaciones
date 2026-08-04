using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;

namespace Vacations.Infrastructure.Persistence.Repositories;

public class RepositorioSaldoEmpleado : IRepositorioSaldoEmpleado
{
    private readonly VacacionesDbContext _context;

    public RepositorioSaldoEmpleado(VacacionesDbContext context)
    {
        _context = context;
    }

    public async Task<SaldoEmpleado?> ObtenerPorEmpleadoIdAsync(Guid empleadoId, CancellationToken cancellationToken = default)
    {
        return await _context.SaldosEmpleado
            .FirstOrDefaultAsync(s => s.EmpleadoId == empleadoId, cancellationToken);
    }

    public async Task AgregarAsync(SaldoEmpleado saldo, CancellationToken cancellationToken = default)
    {
        await _context.SaldosEmpleado.AddAsync(saldo, cancellationToken);
    }

    public void Actualizar(SaldoEmpleado saldo)
    {
        _context.SaldosEmpleado.Update(saldo);
    }

    public async Task<IReadOnlyList<SaldoEmpleado>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaldosEmpleado
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
