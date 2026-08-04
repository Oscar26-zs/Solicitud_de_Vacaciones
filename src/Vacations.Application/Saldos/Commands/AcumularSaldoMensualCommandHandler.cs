using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;

namespace Vacations.Application.Saldos.Commands;

public sealed class AcumularSaldoMensualCommandHandler
{
    private readonly IRepositorioEmpleado _repositorioEmpleados;
    private readonly IRepositorioSaldoEmpleado _repositorioSaldos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public AcumularSaldoMensualCommandHandler(
        IRepositorioEmpleado repositorioEmpleados,
        IRepositorioSaldoEmpleado repositorioSaldos,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _repositorioEmpleados = repositorioEmpleados;
        _repositorioSaldos = repositorioSaldos;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<int> HandleAsync(CancellationToken cancellationToken = default)
    {
        var ahora = _timeProvider.GetUtcNow().DateTime;
        var fechaActual = DateOnly.FromDateTime(ahora);

        var empleadosActivos = await _repositorioEmpleados.ObtenerActivosAsync(cancellationToken);
        var diasAcumuladosTotal = 0;

        foreach (var empleado in empleadosActivos)
        {
            var saldo = await _repositorioSaldos.ObtenerPorEmpleadoIdAsync(empleado.Id, cancellationToken);

            if (saldo == null)
            {
                saldo = SaldoEmpleado.Crear(empleado.Id, ahora);
                await _repositorioSaldos.AgregarAsync(saldo, cancellationToken);
            }

            var mesesCompletos = CalcularMesesCompletosLaborados(empleado.FechaIngreso, fechaActual);
            var diasYaAcumulados = saldo.SaldoAcumulado;
            var diasPendientesAcumular = mesesCompletos - diasYaAcumulados;

            if (diasPendientesAcumular > 0)
            {
                saldo.AcumularDias(diasPendientesAcumular, ahora);
                _repositorioSaldos.Actualizar(saldo);
                diasAcumuladosTotal += diasPendientesAcumular;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return diasAcumuladosTotal;
    }

    private static int CalcularMesesCompletosLaborados(DateOnly fechaIngreso, DateOnly fechaActual)
    {
        if (fechaActual < fechaIngreso)
        {
            return 0;
        }

        var meses = (fechaActual.Year - fechaIngreso.Year) * 12 + fechaActual.Month - fechaIngreso.Month;

        if (fechaActual.Day < fechaIngreso.Day)
        {
            meses--;
        }

        return Math.Max(0, meses);
    }
}
