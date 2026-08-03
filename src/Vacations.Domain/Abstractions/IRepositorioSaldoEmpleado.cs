using Vacations.Domain.Entities;

namespace Vacations.Domain.Abstractions;

/// <summary>
/// Repositorio para la entidad <see cref="SaldoEmpleado"/>.
/// </summary>
public interface IRepositorioSaldoEmpleado
{
    Task<SaldoEmpleado?> ObtenerPorEmpleadoIdAsync(Guid empleadoId, CancellationToken cancellationToken = default);

    Task AgregarAsync(SaldoEmpleado saldo, CancellationToken cancellationToken = default);

    Task ActualizarAsync(SaldoEmpleado saldo, CancellationToken cancellationToken = default);
}