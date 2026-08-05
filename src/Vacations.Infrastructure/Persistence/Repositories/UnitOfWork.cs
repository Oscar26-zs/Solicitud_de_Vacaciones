using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Exceptions;
using Vacations.Infrastructure.Persistence;

namespace Vacations.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly VacacionesDbContext _context;

    public UnitOfWork(VacacionesDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrenciaException();
        }
    }
}