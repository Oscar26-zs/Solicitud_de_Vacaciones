using NSubstitute;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Tests.Solicitudes;

public class ObtenerSolicitudesRRHHQueryHandlerTests
{
    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();
    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();

    private static SolicitudVacaciones Crear(Guid empleadoId) =>
        SolicitudVacaciones.Crear(
            empleadoId,
            RangoFechas.Crear(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 14), new DateOnly(2026, 8, 3)),
            "Vacaciones familiares",
            new DateTime(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task HandleAsync_ConSolicitudes_MapeaConEmpleado()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var solicitudesBase = new List<SolicitudVacaciones> { Crear(empleadoId) };
        IReadOnlyList<SolicitudVacaciones> solicitudes = solicitudesBase;

        _solRepo.ObtenerParaRRHHAsync(
            Arg.Any<Guid?>(), Arg.Any<EstadoSolicitud?>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            1, 10, Arg.Any<CancellationToken>()).Returns((solicitudes, 1));
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(
            Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 1)));

        var handler = new ObtenerSolicitudesRRHHQueryHandler(_solRepo, _empRepo);

        // Act
        var resultado = await handler.HandleAsync(
            new ObtenerSolicitudesRRHHQuery(empleadoId, EstadoSolicitud.Pending, null, null, 1, 10),
            CancellationToken.None);

        // Assert
        Assert.Single(resultado.Solicitudes);
        Assert.Equal("Juan Pérez", resultado.Solicitudes[0].EmpleadoNombre);
        Assert.Equal(EstadoSolicitud.Pending, resultado.Solicitudes[0].Estado);
        Assert.Equal(1, resultado.TotalCount);
        Assert.Equal(1, resultado.TotalPages);
    }

    [Fact]
    public async Task HandleAsync_EmpleadoInexistente_UsaDesconocido()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var solicitudesBase = new List<SolicitudVacaciones> { Crear(empleadoId) };
        IReadOnlyList<SolicitudVacaciones> solicitudes = solicitudesBase;

        _solRepo.ObtenerParaRRHHAsync(
            Arg.Any<Guid?>(), Arg.Any<EstadoSolicitud?>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>(),
            1, 10, Arg.Any<CancellationToken>()).Returns((solicitudes, 1));
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns((Empleado?)null);

        var handler = new ObtenerSolicitudesRRHHQueryHandler(_solRepo, _empRepo);

        // Act
        var resultado = await handler.HandleAsync(
            new ObtenerSolicitudesRRHHQuery(), CancellationToken.None);

        // Assert
        Assert.Equal("Desconocido", resultado.Solicitudes[0].EmpleadoNombre);
    }
}