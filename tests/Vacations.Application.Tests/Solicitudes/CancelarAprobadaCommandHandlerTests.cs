using NSubstitute;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Tests.Solicitudes;

public class CancelarAprobadaCommandHandlerTests
{
    private static readonly DateTime _now = new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);

    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();
    private readonly IRepositorioSaldoEmpleado _saldoRepo = Substitute.For<IRepositorioSaldoEmpleado>();
    private readonly IRepositorioHistorialSolicitud _histRepo = Substitute.For<IRepositorioHistorialSolicitud>();
    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider;

    public CancelarAprobadaCommandHandlerTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(_now, TimeSpan.Zero));
    }

    private CancelarAprobadaCommandHandler CrearHandler() =>
        new(_solRepo, _saldoRepo, _histRepo, _empRepo, _uow, _timeProvider);

    private static SolicitudVacaciones CrearSolicitudAprobada(Guid empleadoId, DateOnly inicio, DateOnly fin)
    {
        var solicitud = SolicitudVacaciones.Crear(
            empleadoId,
            RangoFechas.CrearSinValidacion(inicio, fin),
            "Vacaciones familiares",
            _now);
        solicitud.Aprobar(Guid.NewGuid(), _now);
        return solicitud;
    }

    [Fact]
    public async Task HandleAsync_DatosValidos_CancelaYRestauraSaldoConsumido()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var solicitud = CrearSolicitudAprobada(empleadoId, new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 14));
        var saldo = SaldoEmpleado.Crear(empleadoId, _now);
        saldo.AcumularDias(20, _now);
        saldo.CongelarSaldo(5, _now);
        saldo.DescontarSaldo(5, _now);
        var aprobador = Empleado.Crear("aprobador@example.com", "María García", new DateOnly(2023, 1, 1));

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(aprobadorId, Arg.Any<CancellationToken>()).Returns(aprobador);
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(saldo);
        _uow.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = CrearHandler();

        // Act - fecha actual (03/08) < fecha inicio (10/08)
        await handler.HandleAsync(new CancelarAprobadaCommand(solicitud.Id, aprobadorId), CancellationToken.None);

        // Assert
        Assert.Equal(EstadoSolicitud.Cancelled, solicitud.Estado);
        Assert.Equal(0, saldo.SaldoConsumido);
        Assert.Equal(20, saldo.SaldoDisponible);

        _solRepo.Received(1).Actualizar(solicitud);
        await _histRepo.Received(1).AgregarAsync(Arg.Any<HistorialSolicitud>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PeriodoYaIniciado_LanzaCancelacionNoPermitidaException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var solicitud = CrearSolicitudAprobada(empleadoId, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 7));
        var aprobador = Empleado.Crear("aprobador@example.com", "María García", new DateOnly(2023, 1, 1));

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(aprobadorId, Arg.Any<CancellationToken>()).Returns(aprobador);

        var handler = CrearHandler();

        // Act & Assert - fecha inicio (01/08) <= fecha actual (03/08)
        await Assert.ThrowsAsync<CancelacionNoPermitidaException>(() =>
            handler.HandleAsync(new CancelarAprobadaCommand(solicitud.Id, aprobadorId), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_AprobadorInactivo_LanzaAprobadorInactivoException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var solicitud = CrearSolicitudAprobada(empleadoId, new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 14));
        var aprobador = Empleado.Crear("aprobador@example.com", "María García", new DateOnly(2023, 1, 1));
        aprobador.Desactivar();

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(aprobadorId, Arg.Any<CancellationToken>()).Returns(aprobador);

        var handler = CrearHandler();

        // Act & Assert
        await Assert.ThrowsAsync<AprobadorInactivoException>(() =>
            handler.HandleAsync(new CancelarAprobadaCommand(solicitud.Id, aprobadorId), CancellationToken.None));
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
            handler.HandleAsync(new CancelarAprobadaCommand(solicitudId, Guid.NewGuid()), CancellationToken.None));
    }
}
