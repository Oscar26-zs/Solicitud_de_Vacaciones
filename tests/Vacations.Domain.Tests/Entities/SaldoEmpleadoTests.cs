using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;

namespace Vacations.Domain.Tests.Entities;

public class SaldoEmpleadoTests
{
    private static readonly DateTime Hoy = new(2026, 8, 3);

    [Fact]
    public void AcumularDias_IncrementaSaldoAcumulado()
    {
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), Hoy);

        saldo.AcumularDias(5, Hoy);

        Assert.Equal(5, saldo.SaldoAcumulado);
        Assert.Equal(5, saldo.SaldoDisponible);
    }

    [Fact]
    public void CongelarSaldo_IncrementaPendienteYReduceDisponible()
    {
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), Hoy);
        saldo.AcumularDias(10, Hoy);

        saldo.CongelarSaldo(3, Hoy);

        Assert.Equal(3, saldo.SaldoPendiente);
        Assert.Equal(7, saldo.SaldoDisponible);
    }

    [Fact]
    public void DescontarSaldo_MueveDePendienteAConsumido()
    {
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), Hoy);
        saldo.AcumularDias(10, Hoy);
        saldo.CongelarSaldo(4, Hoy);

        saldo.DescontarSaldo(4, Hoy);

        Assert.Equal(0, saldo.SaldoPendiente);
        Assert.Equal(4, saldo.SaldoConsumido);
        Assert.Equal(6, saldo.SaldoDisponible);
    }

    [Fact]
    public void LiberarSaldoPendiente_RestauraDisponible()
    {
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), Hoy);
        saldo.AcumularDias(10, Hoy);
        saldo.CongelarSaldo(4, Hoy);

        saldo.LiberarSaldoPendiente(4, Hoy);

        Assert.Equal(0, saldo.SaldoPendiente);
        Assert.Equal(10, saldo.SaldoDisponible);
    }

    [Fact]
    public void RestaurarSaldo_DevuelveDiasConsumidos()
    {
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), Hoy);
        saldo.AcumularDias(10, Hoy);
        saldo.CongelarSaldo(4, Hoy);
        saldo.DescontarSaldo(4, Hoy);

        saldo.RestaurarSaldo(4, Hoy);

        Assert.Equal(0, saldo.SaldoConsumido);
        Assert.Equal(10, saldo.SaldoDisponible);
    }

    [Fact]
    public void CongelarSaldoConSaldoInsuficiente_LanzaSaldoInsuficiente()
    {
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), Hoy);
        saldo.AcumularDias(2, Hoy);

        Assert.Throws<SaldoInsuficienteException>(() => saldo.CongelarSaldo(5, Hoy));
    }
}