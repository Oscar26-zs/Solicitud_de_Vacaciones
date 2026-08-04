using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;

namespace Vacations.Domain.Tests.Entities;

public class SaldoEmpleadoTests
{
    private readonly DateTime _fechaActual = DateTime.UtcNow;

    [Fact]
    public void Crear_RetornaSaldoConValoresCero()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();

        // Act
        var saldo = SaldoEmpleado.Crear(empleadoId, _fechaActual);

        // Assert
        Assert.Equal(empleadoId, saldo.EmpleadoId);
        Assert.Equal(0, saldo.SaldoAcumulado);
        Assert.Equal(0, saldo.SaldoConsumido);
        Assert.Equal(0, saldo.SaldoPendiente);
        Assert.Equal(0, saldo.SaldoDisponible);
    }

    [Fact]
    public void AcumularDias_IncrementaSaldoAcumulado()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);

        // Act
        saldo.AcumularDias(10, _fechaActual);

        // Assert
        Assert.Equal(10, saldo.SaldoAcumulado);
        Assert.Equal(10, saldo.SaldoDisponible);
    }

    [Fact]
    public void AcumularDias_ConValorCeroONegativo_LanzaArgumentException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => saldo.AcumularDias(0, _fechaActual));
        Assert.Throws<ArgumentException>(() => saldo.AcumularDias(-5, _fechaActual));
    }

    [Fact]
    public void CongelarSaldo_ReduceSaldoDisponible()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(20, _fechaActual);

        // Act
        saldo.CongelarSaldo(5, _fechaActual);

        // Assert
        Assert.Equal(20, saldo.SaldoAcumulado);
        Assert.Equal(5, saldo.SaldoPendiente);
        Assert.Equal(15, saldo.SaldoDisponible);
    }

    [Fact]
    public void CongelarSaldo_SinSaldoSuficiente_LanzaSaldoInsuficienteException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(5, _fechaActual);

        // Act & Assert
        Assert.Throws<SaldoInsuficienteException>(() => saldo.CongelarSaldo(10, _fechaActual));
    }

    [Fact]
    public void DescontarSaldo_MovimientoDePendienteAConsumido()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(20, _fechaActual);
        saldo.CongelarSaldo(5, _fechaActual);

        // Act
        saldo.DescontarSaldo(5, _fechaActual);

        // Assert
        Assert.Equal(5, saldo.SaldoConsumido);
        Assert.Equal(0, saldo.SaldoPendiente);
        Assert.Equal(15, saldo.SaldoDisponible);
    }

    [Fact]
    public void LiberarSaldoPendiente_DevuelveSaldoADisponible()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(20, _fechaActual);
        saldo.CongelarSaldo(5, _fechaActual);

        // Act
        saldo.LiberarSaldoPendiente(5, _fechaActual);

        // Assert
        Assert.Equal(0, saldo.SaldoPendiente);
        Assert.Equal(20, saldo.SaldoDisponible);
    }

    [Fact]
    public void RestaurarSaldo_DevuelveSaldoConsumidoADisponible()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(20, _fechaActual);
        saldo.CongelarSaldo(5, _fechaActual);
        saldo.DescontarSaldo(5, _fechaActual);

        // Act
        saldo.RestaurarSaldo(5, _fechaActual);

        // Assert
        Assert.Equal(0, saldo.SaldoConsumido);
        Assert.Equal(20, saldo.SaldoDisponible);
    }

    [Fact]
    public void SaldoDisponible_CalculaCorrectamente()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(30, _fechaActual);
        saldo.CongelarSaldo(5, _fechaActual);
        saldo.DescontarSaldo(5, _fechaActual);
        saldo.CongelarSaldo(3, _fechaActual);

        // Assert
        // Disponible = 30 (acumulado) - 5 (consumido) - 3 (pendiente) = 22
        Assert.Equal(22, saldo.SaldoDisponible);
    }

    [Fact]
    public void CongelarSaldo_ConValorCeroONegativo_LanzaArgumentException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => saldo.CongelarSaldo(0, _fechaActual));
        Assert.Throws<ArgumentException>(() => saldo.CongelarSaldo(-3, _fechaActual));
    }

    [Fact]
    public void DescontarSaldo_ConValorCeroONegativo_LanzaArgumentException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => saldo.DescontarSaldo(0, _fechaActual));
        Assert.Throws<ArgumentException>(() => saldo.DescontarSaldo(-2, _fechaActual));
    }

    [Fact]
    public void DescontarSaldo_MasDiasQuePendientes_LanzaInvalidOperationException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(10, _fechaActual);
        saldo.CongelarSaldo(4, _fechaActual);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => saldo.DescontarSaldo(5, _fechaActual));
    }

    [Fact]
    public void LiberarSaldoPendiente_ConValorCeroONegativo_LanzaArgumentException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => saldo.LiberarSaldoPendiente(0, _fechaActual));
        Assert.Throws<ArgumentException>(() => saldo.LiberarSaldoPendiente(-1, _fechaActual));
    }

    [Fact]
    public void LiberarSaldoPendiente_MasDiasQuePendientes_LanzaInvalidOperationException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(10, _fechaActual);
        saldo.CongelarSaldo(3, _fechaActual);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => saldo.LiberarSaldoPendiente(4, _fechaActual));
    }

    [Fact]
    public void RestaurarSaldo_ConValorCeroONegativo_LanzaArgumentException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => saldo.RestaurarSaldo(0, _fechaActual));
        Assert.Throws<ArgumentException>(() => saldo.RestaurarSaldo(-1, _fechaActual));
    }

    [Fact]
    public void RestaurarSaldo_MasDiasQueConsumidos_LanzaInvalidOperationException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(10, _fechaActual);
        saldo.CongelarSaldo(3, _fechaActual);
        saldo.DescontarSaldo(3, _fechaActual);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => saldo.RestaurarSaldo(4, _fechaActual));
    }

    [Fact]
    public void AjustarSaldoPendiente_IncrementoAumentaPendiente()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(20, _fechaActual);
        saldo.CongelarSaldo(5, _fechaActual);

        // Act - de 5 a 8 días (incremento de 3)
        saldo.AjustarSaldoPendiente(5, 8, _fechaActual);

        // Assert
        Assert.Equal(8, saldo.SaldoPendiente);
        Assert.Equal(12, saldo.SaldoDisponible);
    }

    [Fact]
    public void AjustarSaldoPendiente_DecrementoReducePendiente()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(20, _fechaActual);
        saldo.CongelarSaldo(8, _fechaActual);

        // Act - de 8 a 5 días (decremento de 3)
        saldo.AjustarSaldoPendiente(8, 5, _fechaActual);

        // Assert
        Assert.Equal(5, saldo.SaldoPendiente);
        Assert.Equal(15, saldo.SaldoDisponible);
    }

    [Fact]
    public void AjustarSaldoPendiente_SinCambio_NoModificaPendiente()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(20, _fechaActual);
        saldo.CongelarSaldo(5, _fechaActual);

        // Act
        saldo.AjustarSaldoPendiente(5, 5, _fechaActual);

        // Assert
        Assert.Equal(5, saldo.SaldoPendiente);
        Assert.Equal(15, saldo.SaldoDisponible);
    }

    [Fact]
    public void AjustarSaldoPendiente_SaldoInsuficiente_LanzaSaldoInsuficienteException()
    {
        // Arrange
        var saldo = SaldoEmpleado.Crear(Guid.NewGuid(), _fechaActual);
        saldo.AcumularDias(5, _fechaActual);
        saldo.CongelarSaldo(4, _fechaActual); // Disponible = 1

        // Act & Assert - incremento de 3 días requiere 3 disponibles
        Assert.Throws<SaldoInsuficienteException>(() => saldo.AjustarSaldoPendiente(4, 7, _fechaActual));
    }
}
