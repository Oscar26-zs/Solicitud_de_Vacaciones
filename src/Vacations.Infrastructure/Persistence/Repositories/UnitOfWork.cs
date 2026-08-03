using Vacations.Domain.Abstractions;
using Vacations.Infrastructure.Persistence;

namespace Vacations.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly VacacionesDbContext _context;

    public UnitOfWork(VacacionesDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}