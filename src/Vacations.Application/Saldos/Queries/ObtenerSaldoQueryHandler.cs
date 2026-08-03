using Vacations.Application.Common;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Saldos.Queries;

/// <summary>
/// Handler del caso de uso CU-02: consulta el saldo de un empleado. Permite a un
/// empleado consultar su propio saldo y a RRHH el de cualquier empleado.
/// </summary>
public sealed class ObtenerSaldoQueryHandler
{
    private readonly IRepositorioSaldoEmpleado _saldos;

    public ObtenerSaldoQueryHandler(IRepositorioSaldoEmpleado saldos)
    {
        _saldos = saldos;
    }

    public async Task<SaldoDto> HandleAsync(ObtenerSaldoQuery query, CancellationToken cancellationToken = default)
    {
        if (!query.EsRRHH && query.EmpleadoSolicitanteId != query.EmpleadoId)
        {
            throw new AccesoNoPermitidoException("Solo RRHH puede consultar el saldo de otro empleado.");
        }

        var saldo = await _saldos.ObtenerPorEmpleadoIdAsync(query.EmpleadoId, cancellationToken)
            ?? throw new SaldoNoEncontradoException($"No se encontró saldo para el empleado {query.EmpleadoId}.");

        return new SaldoDto
        {
            EmpleadoId = saldo.EmpleadoId,
            Acumulado = saldo.SaldoAcumulado,
            Consumido = saldo.SaldoConsumido,
            Pendiente = saldo.SaldoPendiente,
            Disponible = saldo.SaldoDisponible,
        };
    }
}