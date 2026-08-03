using Moq;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Tests.Solicitudes;

public class CrearSolicitudCommandHandlerTests
{
    private static readonly DateTime Hoy = new(2026, 8, 3);

    private readonly Mock<IRepositorioSolicitudVacaciones> _solicitudes = new();
    private readonly Mock<IRepositorioSaldoEmpleado> _saldos = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TimeProvider _timeProvider;

    private readonly Guid _empleadoId = Guid.NewGuid();

    public CrearSolicitudCommandHandlerTests()
    {
        var t = new TimeProviderFalso();
        _timeProvider = t;
        t.SetNow(Hoy);
    }

    private CrearSolicitudCommandHandler CrearHandler()
        => new(_solicitudes.Object, _saldos.Object, _unitOfWork.Object, _timeProvider);

    private CrearSolicitudCommand ComandoValido()
        => new()
        {
            EmpleadoId = _empleadoId,
            FechaInicio = Hoy.AddDays(10), // lunes aprox. (16)
            FechaFin = Hoy.AddDays(14),
            Motivo = "Vacaciones de descanso anual",
        };

    private SaldoEmpleado SaldoSuficiente()
    {
        var saldo = SaldoEmpleado.Crear(_empleadoId, Hoy);
        saldo.AcumularDias(20, Hoy);
        return saldo;
    }

    [Fact]
    public async Task CrearConSaldoSuficiente_DevuelveSolicitudYCongelaSaldo()
    {
        var saldo = SaldoSuficiente();
        _saldos.Setup(s => s.ObtenerPorEmpleadoIdAsync(_empleadoId, It.IsAny<CancellationToken>())).ReturnsAsync(saldo);
        _solicitudes.Setup(s => s.ExisteTraslapeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = CrearHandler();
        var resultado = await handler.HandleAsync(ComandoValido());

        Assert.NotEqual(Guid.Empty, resultado.SolicitudId);
        _solicitudes.Verify(s => s.AgregarAsync(It.IsAny<SolicitudVacaciones>(), It.IsAny<CancellationToken>()), Times.Once);
        _saldos.Verify(s => s.ActualizarAsync(It.IsAny<SaldoEmpleado>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(saldo.SaldoPendiente > 0);
    }

    [Fact]
    public async Task CrearConSaldoInsuficiente_LanzaSaldoInsuficiente()
    {
        var saldo = SaldoEmpleado.Crear(_empleadoId, Hoy);
        saldo.AcumularDias(1, Hoy);
        _saldos.Setup(s => s.ObtenerPorEmpleadoIdAsync(_empleadoId, It.IsAny<CancellationToken>())).ReturnsAsync(saldo);

        var handler = CrearHandler();

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() => handler.HandleAsync(ComandoValido()));
        _solicitudes.Verify(s => s.AgregarAsync(It.IsAny<SolicitudVacaciones>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearConTraslape_LanzaTraslape()
    {
        var saldo = SaldoSuficiente();
        _saldos.Setup(s => s.ObtenerPorEmpleadoIdAsync(_empleadoId, It.IsAny<CancellationToken>())).ReturnsAsync(saldo);
        _solicitudes.Setup(s => s.ExisteTraslapeAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = CrearHandler();

        await Assert.ThrowsAsync<TraslapeSolicitudesException>(() => handler.HandleAsync(ComandoValido()));
        _solicitudes.Verify(s => s.AgregarAsync(It.IsAny<SolicitudVacaciones>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class TimeProviderFalso : TimeProvider
    {
        private DateTimeOffset _now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void SetNow(DateTime now) => _now = new DateTimeOffset(DateTime.SpecifyKind(now, DateTimeKind.Utc));
    }
}