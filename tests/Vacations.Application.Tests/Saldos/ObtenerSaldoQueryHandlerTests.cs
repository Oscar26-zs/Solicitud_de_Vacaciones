using NSubstitute;
using Vacations.Application.Saldos.Queries;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;

namespace Vacations.Application.Tests.Saldos;

public class ObtenerSaldoQueryHandlerTests
{
    private readonly IRepositorioSaldoEmpleado _saldoRepo = Substitute.For<IRepositorioSaldoEmpleado>();
    private readonly DateTime _fecha = new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_ConSaldo_RetornaDtoMapeado()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var saldo = SaldoEmpleado.Crear(empleadoId, _fecha);
        saldo.AcumularDias(20, _fecha);
        saldo.CongelarSaldo(5, _fecha);

        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(saldo);

        var handler = new ObtenerSaldoQueryHandler(_saldoRepo);

        // Act
        var resultado = await handler.HandleAsync(new ObtenerSaldoQuery(empleadoId), CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(20, resultado!.SaldoAcumulado);
        Assert.Equal(0, resultado.SaldoConsumido);
        Assert.Equal(5, resultado.SaldoPendiente);
        Assert.Equal(15, resultado.SaldoDisponible);
    }

    [Fact]
    public async Task HandleAsync_SinSaldo_RetornaNull()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns((SaldoEmpleado?)null);

        var handler = new ObtenerSaldoQueryHandler(_saldoRepo);

        // Act
        var resultado = await handler.HandleAsync(new ObtenerSaldoQuery(empleadoId), CancellationToken.None);

        // Assert
        Assert.Null(resultado);
    }
}