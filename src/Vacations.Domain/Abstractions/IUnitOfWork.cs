namespace Vacations.Domain.Abstractions;

/// <summary>
/// Unidad de trabajo que garantiza consistencia transaccional de los cambios
/// persistidos dentro de una operación (constitution §3.3).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}