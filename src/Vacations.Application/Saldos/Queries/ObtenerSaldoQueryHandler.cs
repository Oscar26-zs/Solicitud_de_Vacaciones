using Vacations.Domain.Abstractions;

namespace Vacations.Application.Saldos.Queries;

public sealed class ObtenerSaldoQueryHandler
{
    private readonly IRepositorioSaldoEmpleado _repositorioSaldos;

    public ObtenerSaldoQueryHandler(IRepositorioSaldoEmpleado repositorioSaldos)
    {
        _repositorioSaldos = repositorioSaldos;
    }

    public async Task<SaldoDto?> HandleAsync(ObtenerSaldoQuery query, CancellationToken cancellationToken = default)
    {
        var saldo = await _repositorioSaldos.ObtenerPorEmpleadoIdAsync(query.EmpleadoId, cancellationToken);

        if (saldo == null)
        {
            return null;
        }

        return new SaldoDto(
            saldo.SaldoAcumulado,
            saldo.SaldoConsumido,
            saldo.SaldoPendiente,
            saldo.SaldoDisponible,
            saldo.UltimaActualizacion);
    }
}
