using NSubstitute;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Tests.Solicitudes;

public class CancelarSolicitudCommandHandlerTests
{
    private static readonly DateTime _now = new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);

    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();
    private readonly IRepositorioSaldoEmpleado _saldoRepo = Substitute.For<IRepositorioSaldoEmpleado>();
    private readonly IRepositorioHistorialSolicitud _histRepo = Substitute.For<IRepositorioHistorialSolicitud>();
    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider;

    public CancelarSolicitudCommandHandlerTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(_now, TimeSpan.Zero));
    }

    private CancelarSolicitudCommandHandler CrearHandler() =>
        new(_solRepo, _saldoRepo, _histRepo, _empRepo, _uow, _timeProvider);

    private static SolicitudVacaciones CrearSolicitud(Guid empleadoId) =>
        SolicitudVacaciones.Crear(
            empleadoId,
            RangoFechas.Crear(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 14), new DateOnly(2026, 8, 3)),
            "Vacaciones familiares",
            _now);

    [Fact]
    public async Task HandleAsync_DatosValidos_CancelaYLiberaSaldoPendiente()
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
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(saldo);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();

        // Act
        await handler.HandleAsync(new CancelarSolicitudCommand(solicitud.Id, empleadoId), CancellationToken.None);

        // Assert
        Assert.Equal(EstadoSolicitud.Cancelled, solicitud.Estado);
        Assert.Equal(0, saldo.SaldoPendiente);
        Assert.Equal(20, saldo.SaldoDisponible);

        _solRepo.Received(1).Actualizar(solicitud);
        await _histRepo.Received(1).AgregarAsync(Arg.Any<HistorialSolicitud>(), Arg.Any<CancellationToken>());
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
            handler.HandleAsync(new CancelarSolicitudCommand(solicitud.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_SolicitudAprobada_LanzaCancelacionNoPermitidaException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var solicitud = CrearSolicitud(empleadoId);
        solicitud.Aprobar(Guid.NewGuid(), _now);

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);

        var handler = CrearHandler();

        // Act & Assert
        await Assert.ThrowsAsync<CancelacionNoPermitidaException>(() =>
            handler.HandleAsync(new CancelarSolicitudCommand(solicitud.Id, empleadoId), CancellationToken.None));
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
            handler.HandleAsync(new CancelarSolicitudCommand(solicitudId, Guid.NewGuid()), CancellationToken.None));
    }
}