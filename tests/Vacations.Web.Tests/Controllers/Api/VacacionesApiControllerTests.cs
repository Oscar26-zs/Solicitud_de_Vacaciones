using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.ValueObjects;
using Vacations.Web.Controllers.Api;

namespace Vacations.Web.Tests.Controllers.Api;

public class VacacionesApiControllerTests
{
    private static readonly DateTime _now = new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly _hoy = DateOnly.FromDateTime(_now);
    private static readonly DateOnly _inicio = new(2026, 8, 10);
    private static readonly DateOnly _fin = new(2026, 8, 14);

    private readonly IRepositorioSolicitudVacaciones _solRepo = Substitute.For<IRepositorioSolicitudVacaciones>();
    private readonly IRepositorioSaldoEmpleado _saldoRepo = Substitute.For<IRepositorioSaldoEmpleado>();
    private readonly IRepositorioHistorialSolicitud _histRepo = Substitute.For<IRepositorioHistorialSolicitud>();
    private readonly IRepositorioEmpleado _empRepo = Substitute.For<IRepositorioEmpleado>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<VacacionesApiController> _logger = Substitute.For<ILogger<VacacionesApiController>>();

    public VacacionesApiControllerTests()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(_now, TimeSpan.Zero));
        _histRepo.ObtenerPorSolicitudIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<HistorialSolicitud>());
    }

    private VacacionesApiController CrearController()
    {
        var crearHandler = new CrearSolicitudCommandHandler(_solRepo, _saldoRepo, _histRepo, _empRepo, _uow, _timeProvider);
        var detalleHandler = new ObtenerSolicitudDetalleQueryHandler(_solRepo, _histRepo, _empRepo);
        var validator = new CrearSolicitudCommandValidator(_timeProvider);
        return new VacacionesApiController(crearHandler, validator, detalleHandler, _logger);
    }

    private static Empleado CrearEmpleado() => Empleado.Crear("empleado@example.com", "Juan Perez", new DateOnly(2024, 1, 1));

    private static SaldoEmpleado CrearSaldo(Guid empleadoId, int dias)
    {
        var saldo = SaldoEmpleado.Crear(empleadoId, _now);
        saldo.AcumularDias(dias, _now);
        return saldo;
    }

    [Fact]
    public async Task Solicitar_DatosValidos_Devuelve200ConEstadoPendiente()
    {
        var empleadoId = Guid.NewGuid();
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearEmpleado());
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearSaldo(empleadoId, 20));
        _solRepo.ExisteTraslapeAsync(empleadoId, _inicio, _fin, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);

        var controller = CrearController();
        var request = new SolicitarVacacionesRequest(empleadoId, "Panamá", _inicio, _fin);

        var resultado = await controller.Solicitar(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var body = Assert.IsType<SolicitarVacacionesResponse>(ok.Value);
        Assert.Equal("pendiente", body.Estado);
        Assert.NotEqual(Guid.Empty, body.SolicitudId);
    }

    [Fact]
    public async Task Solicitar_SaldoInsuficiente_Devuelve409ConMensaje()
    {
        var empleadoId = Guid.NewGuid();
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearEmpleado());
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearSaldo(empleadoId, 1));
        _solRepo.ExisteTraslapeAsync(empleadoId, _inicio, _fin, cancellationToken: Arg.Any<CancellationToken>()).Returns(false);

        var controller = CrearController();
        var request = new SolicitarVacacionesRequest(empleadoId, "Panamá", _inicio, _fin);

        var resultado = await controller.Solicitar(request, CancellationToken.None);

        var conflicto = Assert.IsType<ConflictObjectResult>(resultado);
        var body = Assert.IsType<ErrorResponse>(conflicto.Value);
        Assert.Contains("Saldo insuficiente", body.Error);
    }

    [Fact]
    public async Task Solicitar_ConTraslape_Devuelve409()
    {
        var empleadoId = Guid.NewGuid();
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearEmpleado());
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearSaldo(empleadoId, 20));
        _solRepo.ExisteTraslapeAsync(empleadoId, _inicio, _fin, cancellationToken: Arg.Any<CancellationToken>()).Returns(true);

        var controller = CrearController();
        var request = new SolicitarVacacionesRequest(empleadoId, "Panamá", _inicio, _fin);

        var resultado = await controller.Solicitar(request, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(resultado);
    }

    [Fact]
    public async Task Solicitar_FechaInicioEnElPasado_Devuelve400PorValidacion()
    {
        var empleadoId = Guid.NewGuid();
        var controller = CrearController();
        var fechaPasada = _hoy.AddDays(-1);
        var request = new SolicitarVacacionesRequest(empleadoId, "Panamá", fechaPasada, fechaPasada.AddDays(2));

        var resultado = await controller.Solicitar(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.IsType<ErrorResponse>(badRequest.Value);
    }

    [Fact]
    public async Task Solicitar_FechaFinSuperaHorizonteDeDosMeses_Devuelve400EnVezDe500()
    {
        // Caso real encontrado en pruebas manuales: CrearSolicitudCommandValidator
        // solo valida que FechaInicio no supere los 2 meses, pero RangoFechas.Crear
        // valida FechaFin contra ese mismo horizonte y lanza ArgumentException si lo
        // supera. FechaInicio = hoy + 2 meses exactos (pasa el validator) y
        // FechaFin = un poco después (no pasa RangoFechas.Crear) reproduce el hueco.
        var empleadoId = Guid.NewGuid();
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearEmpleado());
        _saldoRepo.ObtenerPorEmpleadoIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearSaldo(empleadoId, 20));

        var controller = CrearController();
        var fechaInicio = _hoy.AddMonths(2);
        var fechaFin = fechaInicio.AddDays(2);
        var request = new SolicitarVacacionesRequest(empleadoId, "Panamá", fechaInicio, fechaFin);

        var resultado = await controller.Solicitar(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.IsType<ErrorResponse>(badRequest.Value);
    }

    [Fact]
    public async Task Estado_SolicitudAprobada_DevuelveEstadoMapeadoAAprobada()
    {
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_inicio, _fin, _hoy);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Viaje a Panamá", _now);
        solicitud.Aprobar(aprobadorId, _now);

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearEmpleado());

        var controller = CrearController();

        var resultado = await controller.Estado(solicitud.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var body = Assert.IsType<EstadoSolicitudResponse>(ok.Value);
        Assert.Equal("aprobada", body.Estado);
        Assert.Equal(solicitud.Id, body.SolicitudId);
    }

    [Fact]
    public async Task Estado_SolicitudPendiente_DevuelveEstadoMapeadoAPendiente()
    {
        var empleadoId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_inicio, _fin, _hoy);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Viaje a Panamá", _now);

        _solRepo.ObtenerPorIdAsync(solicitud.Id, Arg.Any<CancellationToken>()).Returns(solicitud);
        _empRepo.ObtenerPorIdAsync(empleadoId, Arg.Any<CancellationToken>()).Returns(CrearEmpleado());

        var controller = CrearController();

        var resultado = await controller.Estado(solicitud.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var body = Assert.IsType<EstadoSolicitudResponse>(ok.Value);
        Assert.Equal("pendiente", body.Estado);
    }

    [Fact]
    public async Task Estado_SolicitudInexistente_Devuelve404()
    {
        var solicitudId = Guid.NewGuid();
        _solRepo.ObtenerPorIdAsync(solicitudId, Arg.Any<CancellationToken>()).Returns((SolicitudVacaciones?)null);

        var controller = CrearController();

        var resultado = await controller.Estado(solicitudId, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
