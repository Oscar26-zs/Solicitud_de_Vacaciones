using Vacations.Domain.Entities;

namespace Vacations.Domain.Abstractions;

public interface IRepositorioSaldoEmpleado
{
    Task<SaldoEmpleado?> ObtenerPorEmpleadoIdAsync(Guid empleadoId, CancellationToken cancellationToken = default);

    Task AgregarAsync(SaldoEmpleado saldo, CancellationToken cancellationToken = default);

    void Actualizar(SaldoEmpleado saldo);

    Task<IReadOnlyList<SaldoEmpleado>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
}
