using Vacations.Domain.Entities;

namespace Vacations.Domain.Abstractions;

public interface IRepositorioEmpleado
{
    Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Empleado?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Empleado>> ObtenerActivosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Empleado>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    Task AgregarAsync(Empleado empleado, CancellationToken cancellationToken = default);

    void Actualizar(Empleado empleado);

    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}
