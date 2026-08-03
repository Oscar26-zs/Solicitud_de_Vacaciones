using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Infrastructure.Persistence;

namespace Vacations.Infrastructure.Persistence.Repositories;

public sealed class RepositorioSaldoEmpleado : IRepositorioSaldoEmpleado
{
    private readonly VacacionesDbContext _context;

    public RepositorioSaldoEmpleado(VacacionesDbContext context)
    {
        _context = context;
    }

    public async Task<SaldoEmpleado?> ObtenerPorEmpleadoIdAsync(Guid empleadoId, CancellationToken cancellationToken = default)
        => await _context.SaldosEmpleados.FirstOrDefaultAsync(s => s.EmpleadoId == empleadoId, cancellationToken);

    public async Task AgregarAsync(SaldoEmpleado saldo, CancellationToken cancellationToken = default)
        => await _context.SaldosEmpleados.AddAsync(saldo, cancellationToken);

    public Task ActualizarAsync(SaldoEmpleado saldo, CancellationToken cancellationToken = default)
    {
        _context.SaldosEmpleados.Update(saldo);
        return Task.CompletedTask;
    }
}