using NSubstitute;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Tests.Solicitudes;

public class ObtenerMisSolicitudesQueryHandlerTests
{
    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();

    private static SolicitudVacaciones Crear(Guid empleadoId, EstadoSolicitud estado) =>
        SolicitudVacaciones.Crear(
            empleadoId,
            RangoFechas.Crear(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 14), new DateOnly(2026, 8, 3)),
            "Vacaciones familiares",
            new DateTime(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc));

    [Fact]
    public async Task HandleAsync_ConPageSizeValido_MapeaSolicitudesYPaginacion()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var lista = new List<SolicitudVacaciones> { Crear(empleadoId, EstadoSolicitud.Pending) };
        IReadOnlyList<SolicitudVacaciones> solicitudes = lista;

        _solRepo.ObtenerPorEmpleadoPaginadoAsync(
            empleadoId, Arg.Any<EstadoSolicitud?>(), 1, 5, Arg.Any<CancellationToken>())
            .Returns((solicitudes, 1));

        var handler = new ObtenerMisSolicitudesQueryHandler(_solRepo);

        // Act
        var resultado = await handler.HandleAsync(
            new ObtenerMisSolicitudesQuery(empleadoId, null, 1, 5), CancellationToken.None);

        // Assert
        Assert.Single(resultado.Solicitudes);
        Assert.Equal(5, resultado.PageSize);
        Assert.Equal(1, resultado.TotalCount);
        Assert.Equal(1, resultado.TotalPages);
        Assert.Equal(EstadoSolicitud.Pending, resultado.Solicitudes[0].Estado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(100)]
    public async Task HandleAsync_ConPageSizeInvalido_UsaDefault10(int pageSize)
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        IReadOnlyList<SolicitudVacaciones> solicitudes = new List<SolicitudVacaciones>();

        _solRepo.ObtenerPorEmpleadoPaginadoAsync(
            empleadoId, Arg.Any<EstadoSolicitud?>(), 1, 10, Arg.Any<CancellationToken>())
            .Returns((solicitudes, 0));

        var handler = new ObtenerMisSolicitudesQueryHandler(_solRepo);

        // Act
        var resultado = await handler.HandleAsync(
            new ObtenerMisSolicitudesQuery(empleadoId, null, 1, pageSize), CancellationToken.None);

        // Assert
        Assert.Equal(10, resultado.PageSize);
    }

    [Fact]
    public async Task HandleAsync_PageMenorAUno_UsaPaginaUno()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        IReadOnlyList<SolicitudVacaciones> solicitudes = new List<SolicitudVacaciones>();

        _solRepo.ObtenerPorEmpleadoPaginadoAsync(
            empleadoId, Arg.Any<EstadoSolicitud?>(), 1, 10, Arg.Any<CancellationToken>())
            .Returns((solicitudes, 0));

        var handler = new ObtenerMisSolicitudesQueryHandler(_solRepo);

        // Act
        var resultado = await handler.HandleAsync(
            new ObtenerMisSolicitudesQuery(empleadoId, null, 0, 10), CancellationToken.None);

        // Assert
        Assert.Equal(1, resultado.Page);
    }
}