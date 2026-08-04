using NSubstitute;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Tests.Solicitudes;

public class RechazarSolicitudCommandHandlerTests
{
    private static readonly DateTime _now = new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly _inicio = new(2026, 8, 10);
    private static readonly DateOnly _fin = new(2026, 8, 14);

    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();
    private readonly IRepositorioSaldoEmpleado _saldoRepo = Substitute.For<IRepositorioSaldoEmpleado>();
    private readonly IRepositorioHistorialSolicitud _histRepo = Substitute.For<IRepositorioHistorialSolicitud>();
    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider;

    public RechazarSolicitudCommandHandlerTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(_now, TimeSpan.Zero));
    }

    private RechazarSolicitudCommandHandler CrearHandler() =>
        new(_solRepo, _saldoRepo, _histRepo, _empRepo, _uow, _timeProvider);

    private static SolicitudVacaciones CrearSolicitud(Guid empleadoId) =>
        SolicitudVacaciones.Crear(
            empleadoId,
            RangoFechas.Crear(_inicio, _fin, new DateOnly(2026, 8, 3)),
            "Vacaciones familiares",
            _now);

    private static SaldoEmpleado CrearSaldoConPendiente(Guid empleadoId)
    {
        var saldo = SaldoEmpleado.Crear(empleadoId, _now);
        saldo.AcumularDias(20, _now);
        saldo.CongelarSaldo(5, _now);
        return saldo;
    }

    [Fact]
    public async Task HandleAsync_DatosValidos_LiberaSaldoPendienteYRegistraHistorial()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var solicitud = CrearSolicitud(empleadoId);
        var saldo = CrearSaldoConPendiente(empleadoId);
        var aprobador = Empleado.Crear("aprobador@example.com", "María García", new DateOnly(2023, 1, 1));

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(aprobadorId, Arg.Any<CancellationToken>()).Returns(aprobador);
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(saldo);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();

        // Act
        await handler.HandleAsync(
            new RechazarSolicitudCommand(solicitud.Id, aprobadorId, "No hay disponibilidad"),
            CancellationToken.None);

        // Assert
        Assert.Equal(EstadoSolicitud.Rejected, solicitud.Estado);
        Assert.Equal("No hay disponibilidad", solicitud.ComentarioAprobador);
        Assert.Equal(0, saldo.SaldoPendiente);
        Assert.Equal(20, saldo.SaldoDisponible);

        _solRepo.Received(1).Actualizar(solicitud);
        _saldoRepo.Received(1).Actualizar(saldo);
        await _histRepo.Received(1).AgregarAsync(
            Arg.Is<HistorialSolicitud>(h => h.Comentario == "No hay disponibilidad"), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_MismoEmpleadoQueRechaza_LanzaAutoAprobacionNoPermitidaException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var solicitud = CrearSolicitud(empleadoId);
        var empleado = Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 1));

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(empleado);

        var handler = CrearHandler();

        // Act & Assert
        await Assert.ThrowsAsync<AutoAprobacionNoPermitidaException>(() =>
            handler.HandleAsync(new RechazarSolicitudCommand(solicitud.Id, empleadoId, "Comentario"), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_AprobadorInactivo_LanzaAprobadorInactivoException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var solicitud = CrearSolicitud(empleadoId);
        var aprobador = Empleado.Crear("aprobador@example.com", "María García", new DateOnly(2023, 1, 1));
        aprobador.Desactivar();

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(aprobadorId, Arg.Any<CancellationToken>()).Returns(aprobador);

        var handler = CrearHandler();

        // Act & Assert
        await Assert.ThrowsAsync<AprobadorInactivoException>(() =>
            handler.HandleAsync(new RechazarSolicitudCommand(solicitud.Id, aprobadorId, "Comentario"), CancellationToken.None));
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
            handler.HandleAsync(new RechazarSolicitudCommand(solicitudId, Guid.NewGuid(), "Comentario"), CancellationToken.None));
    }
}
