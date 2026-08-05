using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;

namespace Vacations.Application.Saldos.Commands;

/// <summary>Comando para acumular saldo mensual de todos los empleados activos (CU-01).</summary>
public sealed record AcumularSaldoMensualCommand;

/// <summary>
/// Handler del caso de uso CU-01 (job mensual): acumula 1 día por mes completo
/// laborado desde la fecha de ingreso, con carry-over ilimitado (RN-01, RN-23).
/// </summary>
public sealed class AcumularSaldoMensualCommandHandler
{
    private readonly IRepositorioEmpleado _empleados;
    private readonly IRepositorioSaldoEmpleado _saldos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProveedorTiempoCorporativo _proveedorTiempo;

    public AcumularSaldoMensualCommandHandler(
        IRepositorioEmpleado empleados,
        IRepositorioSaldoEmpleado saldos,
        IUnitOfWork unitOfWork,
        IProveedorTiempoCorporativo proveedorTiempo)
    {
        _empleados = empleados;
        _saldos = saldos;
        _unitOfWork = unitOfWork;
        _proveedorTiempo = proveedorTiempo;
    }

    public async Task HandleAsync(AcumularSaldoMensualCommand comando, CancellationToken cancellationToken = default)
    {
        var fechaActual = _proveedorTiempo.ObtenerFechaActualCorporativa();

        var empleados = await _empleados.ObtenerActivosAsync(cancellationToken);

        foreach (var empleado in empleados)
        {
            var mesesCompletos = CalcularMesesCompletos(empleado.FechaIngreso, fechaActual);
            if (mesesCompletos <= 0)
            {
                continue;
            }

            var saldo = await _saldos.ObtenerPorEmpleadoIdAsync(empleado.Id, cancellationToken);
            var esNuevo = saldo is null;
            saldo ??= SaldoEmpleado.Crear(empleado.Id, fechaActual);

            saldo.AcumularDias(mesesCompletos, fechaActual);
            if (esNuevo)
            {
                await _saldos.AgregarAsync(saldo, cancellationToken);
            }
            else
            {
                await _saldos.ActualizarAsync(saldo, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static int CalcularMesesCompletos(DateTime fechaIngreso, DateTime fechaActual)
    {
        var diff = fechaActual - fechaIngreso;
        if (diff.Days < 30)
        {
            return 0;
        }

        var meses = (fechaActual.Year - fechaIngreso.Year) * 12 + (fechaActual.Month - fechaIngreso.Month);
        if (fechaActual.Day < fechaIngreso.Day)
        {
            meses--;
        }

        return Math.Max(0, meses);
    }
}