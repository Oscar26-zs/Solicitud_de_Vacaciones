using NSubstitute;
using Vacations.Application.Saldos.Commands;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;

namespace Vacations.Application.Tests.Saldos;

public class AcumularSaldoMensualCommandHandlerTests
{
    private static readonly DateTime _now = new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);

    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();
    private readonly IRepositorioSaldoEmpleado _saldoRepo = Substitute.For<IRepositorioSaldoEmpleado>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider;

    public AcumularSaldoMensualCommandHandlerTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(_now, TimeSpan.Zero));
    }

    private AcumularSaldoMensualCommandHandler CrearHandler() =>
        new(_empRepo, _saldoRepo, _uow, _timeProvider);

    [Fact]
    public async Task HandleAsync_ConMesPendiente_AcumulaDiasFaltantes()
    {
        // Arrange
        var empleado = Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 1));
        var saldo = SaldoEmpleado.Crear(empleado.Id, _now);
        saldo.AcumularDias(10, _now);

        // FechaIngreso 01/2024 → 08/2026 = 31 meses completos; acumulado 10 → faltan 21
        IReadOnlyList<Empleado> empleados = new List<Empleado> { empleado };
        _empRepo.ObtenerActivosAsync(Arg.Any<CancellationToken>()).Returns(empleados);
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleado.Id, Arg.Any<CancellationToken>()).Returns(saldo);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();

        // Act
        var dias = await handler.HandleAsync(CancellationToken.None);

        // Assert
        Assert.Equal(21, dias);
        Assert.Equal(31, saldo.SaldoAcumulado);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SaldoConTodoAcumulado_NoAcumula()
    {
        // Arrange
        var empleado = Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 1));
        var saldo = SaldoEmpleado.Crear(empleado.Id, _now);
        saldo.AcumularDias(31, _now);

        IReadOnlyList<Empleado> empleados = new List<Empleado> { empleado };
        _empRepo.ObtenerActivosAsync(Arg.Any<CancellationToken>()).Returns(empleados);
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleado.Id, Arg.Any<CancellationToken>()).Returns(saldo);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();

        // Act
        var dias = await handler.HandleAsync(CancellationToken.None);

        // Assert
        Assert.Equal(0, dias);
        Assert.Equal(31, saldo.SaldoAcumulado);
    }

    [Fact]
    public async Task HandleAsync_EmpleadoSinSaldo_CreaSaldo()
    {
        // Arrange
        var empleado = Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 1));

        IReadOnlyList<Empleado> empleados = new List<Empleado> { empleado };
        _empRepo.ObtenerActivosAsync(Arg.Any<CancellationToken>()).Returns(empleados);
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleado.Id, Arg.Any<CancellationToken>()).Returns((SaldoEmpleado?)null);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();

        // Act
        var dias = await handler.HandleAsync(CancellationToken.None);

        // Assert
        Assert.Equal(31, dias);
        await _saldoRepo.Received(1).AgregarAsync(Arg.Any<SaldoEmpleado>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_FechaConDiaAnteriorAlDiaDeIngreso_RestaUnMes()
    {
        // Arrange - Ingreso 15/01/2024, hoy 03/08/2026 (día 3 < 15) → 30 meses
        var empleado = Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 15));
        var saldo = SaldoEmpleado.Crear(empleado.Id, _now);

        IReadOnlyList<Empleado> empleados = new List<Empleado> { empleado };
        _empRepo.ObtenerActivosAsync(Arg.Any<CancellationToken>()).Returns(empleados);
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleado.Id, Arg.Any<CancellationToken>()).Returns(saldo);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();

        // Act
        var dias = await handler.HandleAsync(CancellationToken.None);

        // Assert
        Assert.Equal(30, dias);
        Assert.Equal(30, saldo.SaldoAcumulado);
    }

    [Fact]
    public async Task HandleAsync_FechaIngresoReciente_NoAcumula()
    {
        // Arrange
        var empleado = Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2026, 9, 1));
        var saldo = SaldoEmpleado.Crear(empleado.Id, _now);

        IReadOnlyList<Empleado> empleados = new List<Empleado> { empleado };
        _empRepo.ObtenerActivosAsync(Arg.Any<CancellationToken>()).Returns(empleados);
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleado.Id, Arg.Any<CancellationToken>()).Returns(saldo);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();

        // Act
        var dias = await handler.HandleAsync(CancellationToken.None);

        // Assert
        Assert.Equal(0, dias);
        Assert.Equal(0, saldo.SaldoAcumulado);
    }
}