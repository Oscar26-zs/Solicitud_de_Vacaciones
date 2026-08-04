using NSubstitute;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Tests.Solicitudes;

public class CrearSolicitudCommandHandlerTests
{
    private static readonly DateTime _now = new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly _inicio = new(2026, 8, 10);
    private static readonly DateOnly _fin = new(2026, 8, 14);
    private const int _diasHabiles = 5;

    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();
    private readonly IRepositorioSaldoEmpleado _saldoRepo = Substitute.For<IRepositorioSaldoEmpleado>();
    private readonly IRepositorioHistorialSolicitud _histRepo = Substitute.For<IRepositorioHistorialSolicitud>();
    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider;

    public CrearSolicitudCommandHandlerTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(_now, TimeSpan.Zero));
    }

    private CrearSolicitudCommandHandler CrearHandler() =>
        new(_solRepo, _saldoRepo, _histRepo, _empRepo, _uow, _timeProvider);

    private static Empleado CrearEmpleado(Guid empleadoId) =>
        Empleado.Crear("empleado@example.com", "Juan PÃ©rez", new DateOnly(2024, 1, 1));

    private static SaldoEmpleado CrearSaldo(Guid empleadoId, int dias)
    {
        var saldo = SaldoEmpleado.Crear(empleadoId, _now);
        saldo.AcumularDias(dias, _now);
        return saldo;
    }

    private static CrearSolicitudCommand Comando(Guid empleadoId) =>
        new(empleadoId, _inicio, _fin, "Vacaciones familiares");

    [Fact]
    public async Task HandleAsync_DatosValidos_CreaSolicitudCongelaSaldoYRegistraHistorial()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var empleado = CrearEmpleado(empleadoId);
        var saldo = CrearSaldo(empleadoId, 20);

        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(empleado);
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(saldo);
        _solRepo.ExisteTraslapeAsync(empleadoId, _inicio, _fin, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();
        var command = Comando(empleadoId);

        // Act
        var resultado = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultado);
        Assert.Equal(_diasHabiles, saldo.SaldoPendiente);
        Assert.Equal(15, saldo.SaldoDisponible);

        await _solRepo.Received(1).AgregarAsync(Arg.Any<SolicitudVacaciones>(), Arg.Any<CancellationToken>());
        _saldoRepo.Received(1).Actualizar(saldo);
        await _histRepo.Received(1).AgregarAsync(
            Arg.Is<HistorialSolicitud>(h => h.Actor == empleado.Email), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SaldoInsuficiente_LanzaSaldoInsuficienteException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearEmpleado(empleadoId));
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearSaldo(empleadoId, 3));
        _solRepo.ExisteTraslapeAsync(empleadoId, _inicio, _fin, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);

        var handler = CrearHandler();
        var command = Comando(empleadoId);

        // Act & Assert
        await Assert.ThrowsAsync<SaldoInsuficienteException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_ConTraslape_LanzaTraslapeSolicitudesException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearEmpleado(empleadoId));
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearSaldo(empleadoId, 20));
        _solRepo.ExisteTraslapeAsync(empleadoId, _inicio, _fin, cancellationToken: Arg.Any<CancellationToken>()).Returns(true);

        var handler = CrearHandler();
        var command = Comando(empleadoId);

        // Act & Assert
        await Assert.ThrowsAsync<TraslapeSolicitudesException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_EmpleadoNoEncontrado_LanzaInvalidOperationException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns((Empleado?)null);

        var handler = CrearHandler();
        var command = Comando(empleadoId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_SaldoNoEncontrado_LanzaInvalidOperationException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearEmpleado(empleadoId));
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns((SaldoEmpleado?)null);

        var handler = CrearHandler();
        var command = Comando(empleadoId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command, CancellationToken.None));
    }
}

