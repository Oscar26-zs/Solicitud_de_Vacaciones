using NSubstitute;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.ValueObjects;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Tests.Solicitudes;

public class ObtenerSolicitudDetalleQueryHandlerTests
{
    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();
    private readonly IRepositorioHistorialSolicitud _histRepo = Substitute.For<IRepositorioHistorialSolicitud>();
    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();

    private static SolicitudVacaciones Crear(Guid empleadoId) =>
        SolicitudVacaciones.Crear(
            empleadoId,
            RangoFechas.Crear(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 14), new DateOnly(2026, 8, 3)),
            "Vacaciones familiares",
            new DateTime(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task HandleAsync_DuenoTieneAcceso_RetornaDetalleConHistorial()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var solicitud = Crear(empleadoId);
        var historiales = new List<HistorialSolicitud>
        {
            HistorialSolicitud.CrearParaCreacion(solicitud.Id, "empleado@example.com", DateTime.UtcNow)
        };

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(
            Empleado.Crear("empleado@example.com", "Juan Pérez", new DateOnly(2024, 1, 1)));
        _histRepo.ObtenerPorSolicitudIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(historiales);

        var handler = new ObtenerSolicitudDetalleQueryHandler(_solRepo, _histRepo, _empRepo);

        // Act
        var resultado = await handler.HandleAsync(
            new ObtenerSolicitudDetalleQuery(solicitud.Id, empleadoId, false, false), CancellationToken.None);

        // Assert
        Assert.Equal(solicitud.Id, resultado.Id);
        Assert.Equal("Juan Pérez", resultado.EmpleadoNombre);
        Assert.Single(resultado.Historial);
    }

    [Fact]
    public async Task HandleAsync_HabilitadoParaAprobador_AccesoPermitido()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var solicitud = Crear(empleadoId);

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _histRepo.ObtenerPorSolicitudIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(new List<HistorialSolicitud>());

        var handler = new ObtenerSolicitudDetalleQueryHandler(_solRepo, _histRepo, _empRepo);

        // Act - EsAprobador = true permite acceso a otro empleado
        var resultado = await handler.HandleAsync(
            new ObtenerSolicitudDetalleQuery(solicitud.Id, Guid.NewGuid(), true, false), CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
    }

    [Fact]
    public async Task HandleAsync_SinPermisos_LanzaAccesoNoAutorizadoException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var otroUsuario = Guid.NewGuid();
        var solicitud = Crear(empleadoId);

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);

        var handler = new ObtenerSolicitudDetalleQueryHandler(_solRepo, _histRepo, _empRepo);

        // Act & Assert
        await Assert.ThrowsAsync<AccesoNoAutorizadoException>(() =>
            handler.HandleAsync(
                new ObtenerSolicitudDetalleQuery(solicitud.Id, otroUsuario, false, false), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_SolicitudNoEncontrada_LanzaSolicitudNoEncontradaException()
    {
        // Arrange
        var solicitudId = Guid.NewGuid();
        _solRepo.ObtenerPorIdAsync(solicitudId, Arg.Any<CancellationToken>()).Returns((SolicitudVacaciones?)null);

        var handler = new ObtenerSolicitudDetalleQueryHandler(_solRepo, _histRepo, _empRepo);

        // Act & Assert
        await Assert.ThrowsAsync<SolicitudNoEncontradaException>(() =>
            handler.HandleAsync(
                new ObtenerSolicitudDetalleQuery(solicitudId, Guid.NewGuid(), false, false), CancellationToken.None));
    }
}