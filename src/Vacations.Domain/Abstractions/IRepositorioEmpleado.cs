using Vacations.Domain.Entities;

namespace Vacations.Domain.Abstractions;

/// <summary>
/// Repositorio para la entidad <see cref="Empleado"/>.
/// </summary>
public interface IRepositorioEmpleado
{
    Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Empleado>> ObtenerActivosAsync(CancellationToken cancellationToken = default);

    Task<bool> ExisteConEmailAsync(string email, CancellationToken cancellationToken = default);
}