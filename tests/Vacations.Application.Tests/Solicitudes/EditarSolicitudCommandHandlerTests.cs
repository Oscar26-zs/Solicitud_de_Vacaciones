using NSubstitute;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Tests.Solicitudes;

public class EditarSolicitudCommandHandlerTests
{
    private static readonly DateTime _now = new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly _inicio = new(2026, 8, 10);
    private static readonly DateOnly _fin = new(2026, 8, 14);
    private static readonly DateOnly _finNuevo = new(2026, 8, 19);

    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();
    private readonly IRepositorioSaldoEmpleado _saldoRepo = Substitute.For<IRepositorioSaldoEmpleado>();
    private readonly IRepositorioHistorialSolicitud _histRepo = Substitute.For<IRepositorioHistorialSolicitud>();
    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider;

    public EditarSolicitudCommandHandlerTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(_now, TimeSpan.Zero));
    }

    private EditarSolicitudCommandHandler CrearHandler() =>
        new(_solRepo, _saldoRepo, _histRepo, _empRepo, _uow, _timeProvider);

    private static SolicitudVacaciones CrearSolicitud(Guid empleadoId) =>
        SolicitudVacaciones.Crear(
            empleadoId,
            RangoFechas.Crear(_inicio, _fin, new DateOnly(2026, 8, 3)),
            "Vacaciones familiares",
            _now);

    [Fact]
    public async Task HandleAsync_DatosValidos_EditaAjustaSaldoYRegistraHistorial()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var solicitud = CrearSolicitud(empleadoId);
        var saldo = SaldoEmpleado.Crear(empleadoId, _now);
        saldo.AcumularDias(20, _now);
        saldo.CongelarSaldo(5, _now);
        var empleado = Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 1));

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(empleado);
        _solRepo.ExisteTraslapeAsync(empleadoId, _inicio, _finNuevo, solicitud.Id, Arg.Any<CancellationToken>()).Returns(false);
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(saldo);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();

        // Act - de 5 a 8 días hábiles (incremento de 3)
        await handler.HandleAsync(
            new EditarSolicitudCommand(solicitud.Id, empleadoId, _inicio, _finNuevo, "Vacaciones más largas"),
            CancellationToken.None);

        // Assert
        Assert.Equal(_finNuevo, solicitud.FechaFin);
        Assert.Equal("Vacaciones más largas", solicitud.Motivo);
        Assert.Equal(8, saldo.SaldoPendiente);
        Assert.Equal(12, saldo.SaldoDisponible);

        _solRepo.Received(1).Actualizar(solicitud);
        await _histRepo.Received(1).AgregarAsync(
            Arg.Is<HistorialSolicitud>(h => h.CamposModificados != null), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_OtroEmpleado_LanzaAccesoNoAutorizadoException()
    {
        // Arrange
        var solicitud = CrearSolicitud(Guid.NewGuid());
        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);

        var handler = CrearHandler();

        // Act & Assert
        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(() =>
            handler.HandleAsync(
                new EditarSolicitudCommand(solicitud.Id, Guid.NewGuid(), _inicio, _fin, "Vacaciones"),
                CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_ConTraslape_LanzaTraslapeSolicitudesException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var solicitud = CrearSolicitud(empleadoId);
        var empleado = Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 1));

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(empleado);
        _solRepo.ExisteTraslapeAsync(empleadoId, _inicio, _finNuevo, solicitud.Id, Arg.Any<CancellationToken>()).Returns(true);

        var handler = CrearHandler();

        // Act & Assert
        await Assert.ThrowsAsync<TraslapeSolicitudesException>(() =>
            handler.HandleAsync(
                new EditarSolicitudCommand(solicitud.Id, empleadoId, _inicio, _finNuevo, "Vacaciones más largas"),
                CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_SolicitudNoEncontrada_LanzaSolicitudNoEncontradaException()
    {
        // Arrange
        var solicitudId = Guid.NewGuid();
        _solRepo.ObtenerPorIdAsync(solicitudId, Arg.Any<CancellationToken>()).Returns((SolicitudVacaciones?)null);

        var handler = CrearHandler();

        // Act & Assert
        await Assert.ThrowsAsync<SolicitudNoEncontradaException>(() =>
            handler.HandleAsync(
                new EditarSolicitudCommand(solicitudId, Guid.NewGuid(), _inicio, _fin, "Vacaciones"),
                CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_SolicitudAprobada_LanzaInvalidOperationException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var solicitud = CrearSolicitud(empleadoId);
        solicitud.Aprobar(Guid.NewGuid(), _now);

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);

        var handler = CrearHandler();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(
                new EditarSolicitudCommand(solicitud.Id, empleadoId, _inicio, _finNuevo, "Vacaciones más largas"),
                CancellationToken.None));
    }
}
