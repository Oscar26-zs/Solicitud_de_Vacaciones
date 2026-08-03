using Moq;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Tests.Solicitudes;

public class AprobarSolicitudCommandHandlerTests
{
    private static readonly DateTime Hoy = new(2026, 8, 3);

    private readonly Mock<IRepositorioSolicitudVacaciones> _solicitudes = new();
    private readonly Mock<IRepositorioSaldoEmpleado> _saldos = new();
    private readonly Mock<IRepositorioEmpleado> _empleados = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TimeProvider _timeProvider;

    private readonly Guid _empleadoId = Guid.NewGuid();
    private readonly Guid _aprobadorId = Guid.NewGuid();

    public AprobarSolicitudCommandHandlerTests()
    {
        var t = new TimeProviderFalso();
        _timeProvider = t;
        t.SetNow(Hoy);
    }

    private AprobarSolicitudCommandHandler CrearHandler()
        => new(_solicitudes.Object, _saldos.Object, _empleados.Object, _unitOfWork.Object, _timeProvider);

    private SolicitudVacaciones CrearSolicitud(Guid empleadoId)
        => SolicitudVacaciones.Crear(
            empleadoId,
            Domain.ValueObjects.RangoFechas.Crear(Hoy.AddDays(10), Hoy.AddDays(14), Hoy),
            "Vacaciones de descanso anual",
            Hoy);

    private SaldoEmpleado SaldoConPendiente(SolicitudVacaciones solicitud)
    {
        var saldo = SaldoEmpleado.Crear(solicitud.EmpleadoId, Hoy);
        saldo.AcumularDias(20, Hoy);
        saldo.CongelarSaldo(solicitud.DiasRequeridos, Hoy);
        return saldo;
    }

    [Fact]
    public async Task Aprobar_MueveSaldoDePendienteAConsumido()
    {
        var solicitud = CrearSolicitud(_empleadoId);
        var saldo = SaldoConPendiente(solicitud);
        var aprobador = Empleado.Crear("aprobador@empresa.com", "Aprobador Uno", Hoy.AddYears(-5));

        _solicitudes.Setup(s => s.ObtenerPorIdAsync(solicitud.Id, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        _saldos.Setup(s => s.ObtenerPorEmpleadoIdAsync(_empleadoId, It.IsAny<CancellationToken>())).ReturnsAsync(saldo);
        _empleados.Setup(e => e.ObtenerPorIdAsync(aprobador.Id, It.IsAny<CancellationToken>())).ReturnsAsync(aprobador);

        var handler = CrearHandler();
        await handler.HandleAsync(new AprobarSolicitudCommand { SolicitudId = solicitud.Id, AprobadorEmpleadoId = aprobador.Id });

        Assert.Equal(Domain.Enums.EstadoSolicitud.Approved, solicitud.Estado);
        Assert.Equal(solicitud.DiasRequeridos, saldo.SaldoConsumido);
        Assert.Equal(0, saldo.SaldoPendiente);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AprobarPorAutor_LanzaAutoAprobacion()
    {
        var solicitud = CrearSolicitud(_empleadoId);
        var saldo = SaldoConPendiente(solicitud);
        var aprobador = Empleado.Crear("autor@empresa.com", "El Autor", Hoy.AddYears(-5));
        // El aprobador ES el mismo empleado (mismo Id)
        solicitud = SolicitudVacaciones.Crear(
            aprobador.Id,
            Domain.ValueObjects.RangoFechas.Crear(Hoy.AddDays(10), Hoy.AddDays(14), Hoy),
            "Vacaciones de descanso anual",
            Hoy);
        saldo = SaldoConPendiente(solicitud);

        _solicitudes.Setup(s => s.ObtenerPorIdAsync(solicitud.Id, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        _saldos.Setup(s => s.ObtenerPorEmpleadoIdAsync(aprobador.Id, It.IsAny<CancellationToken>())).ReturnsAsync(saldo);
        _empleados.Setup(e => e.ObtenerPorIdAsync(aprobador.Id, It.IsAny<CancellationToken>())).ReturnsAsync(aprobador);

        var handler = CrearHandler();

        await Assert.ThrowsAsync<AutoAprobacionNoPermitidaException>(() =>
            handler.HandleAsync(new AprobarSolicitudCommand { SolicitudId = solicitud.Id, AprobadorEmpleadoId = aprobador.Id }));
    }

    [Fact]
    public async Task AprobarPorAprobadorInactivo_LanzaAprobadorInactivo()
    {
        var solicitud = CrearSolicitud(_empleadoId);
        var saldo = SaldoConPendiente(solicitud);
        var aprobador = Empleado.Crear("aprobador@empresa.com", "Aprobador Dos", Hoy.AddYears(-5));
        aprobador.Desactivar();

        _solicitudes.Setup(s => s.ObtenerPorIdAsync(solicitud.Id, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        _saldos.Setup(s => s.ObtenerPorEmpleadoIdAsync(_empleadoId, It.IsAny<CancellationToken>())).ReturnsAsync(saldo);
        _empleados.Setup(e => e.ObtenerPorIdAsync(aprobador.Id, It.IsAny<CancellationToken>())).ReturnsAsync(aprobador);

        var handler = CrearHandler();

        await Assert.ThrowsAsync<AprobadorInactivoException>(() =>
            handler.HandleAsync(new AprobarSolicitudCommand { SolicitudId = solicitud.Id, AprobadorEmpleadoId = aprobador.Id }));
    }

    [Fact]
    public async Task AprobarConSaldoInsuficiente_LanzaSaldoInsuficiente()
    {
        var solicitud = CrearSolicitud(_empleadoId);
        var aprobador = Empleado.Crear("aprobador@empresa.com", "Aprobador Tres", Hoy.AddYears(-5));

        // Simula que el saldo disponible ya no alcanza (otro aprobó primero).
        var saldo = SaldoEmpleado.Crear(_empleadoId, Hoy);
        saldo.AcumularDias(1, Hoy);

        _solicitudes.Setup(s => s.ObtenerPorIdAsync(solicitud.Id, It.IsAny<CancellationToken>())).ReturnsAsync(solicitud);
        _saldos.Setup(s => s.ObtenerPorEmpleadoIdAsync(_empleadoId, It.IsAny<CancellationToken>())).ReturnsAsync(saldo);
        _empleados.Setup(e => e.ObtenerPorIdAsync(aprobador.Id, It.IsAny<CancellationToken>())).ReturnsAsync(aprobador);

        var handler = CrearHandler();

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() =>
            handler.HandleAsync(new AprobarSolicitudCommand { SolicitudId = solicitud.Id, AprobadorEmpleadoId = aprobador.Id }));
    }

    private sealed class TimeProviderFalso : TimeProvider
    {
        private DateTimeOffset _now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void SetNow(DateTime now) => _now = new DateTimeOffset(DateTime.SpecifyKind(now, DateTimeKind.Utc));
    }
}