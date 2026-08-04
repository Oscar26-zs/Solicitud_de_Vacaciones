using NSubstitute;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Tests.Solicitudes;

public class ObtenerBandejaAprobadorQueryHandlerTests
{
    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();
    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();
    private readonly IRepositorioSaldoEmpleado _saldoRepo = Substitute.For<IRepositorioSaldoEmpleado>();

    private static SolicitudVacaciones Crear(Guid empleadoId) =>
        SolicitudVacaciones.Crear(
            empleadoId,
            RangoFechas.Crear(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 14), new DateOnly(2026, 8, 3)),
            "Vacaciones familiares",
            new DateTime(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task HandleAsync_ConSolicitudes_IncluyeNombreYSaldoDisponible()
    {
        // Arrange
        var aprobadorId = Guid.NewGuid();
        var empleadoId = Guid.NewGuid();
        var solicitudesBase = new List<SolicitudVacaciones> { Crear(empleadoId) };
        IReadOnlyList<SolicitudVacaciones> solicitudes = solicitudesBase;

        var saldo = SaldoEmpleado.Crear(empleadoId, DateTime.UtcNow);

        _solRepo.ObtenerBandejaAprobadorAsync(
            aprobadorId, Arg.Any<string?>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            1, 5, Arg.Any<CancellationToken>()).Returns((solicitudes, 1));
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(
            Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 1)));
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(saldo);

        var handler = new ObtenerBandejaAprobadorQueryHandler(_solRepo, _empRepo, _saldoRepo);

        // Act
        var resultado = await handler.HandleAsync(
            new ObtenerBandejaAprobadorQuery(aprobadorId, FiltroEmpleado: null, FechaDesde: null, FechaHasta: null, Page: 1, PageSize: 5),
            CancellationToken.None);

        // Assert
        Assert.Single(resultado.Solicitudes);
        Assert.Equal("Juan Pérez", resultado.Solicitudes[0].EmpleadoNombre);
        Assert.Equal(saldo.SaldoDisponible, resultado.Solicitudes[0].SaldoDisponibleEmpleado);
        Assert.Equal(5, resultado.PageSize);
        Assert.Equal(1, resultado.TotalPages);
    }

    [Fact]
    public async Task HandleAsync_PageSizeInvalido_UsaDefault10()
    {
        // Arrange
        var aprobadorId = Guid.NewGuid();
        IReadOnlyList<SolicitudVacaciones> solicitudes = new List<SolicitudVacaciones>();

        _solRepo.ObtenerBandejaAprobadorAsync(
            aprobadorId, Arg.Any<string?>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            1, 10, Arg.Any<CancellationToken>()).Returns((solicitudes, 0));

        var handler = new ObtenerBandejaAprobadorQueryHandler(_solRepo, _empRepo, _saldoRepo);

        // Act
        var resultado = await handler.HandleAsync(
            new ObtenerBandejaAprobadorQuery(aprobadorId, Page: 1, PageSize: 7), CancellationToken.None);

        // Assert
        Assert.Equal(10, resultado.PageSize);
    }
}