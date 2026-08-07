using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Domain.Tests.Entities;

public class SolicitudVacacionesTests
{
    private readonly DateOnly _fechaActual = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Crear_ConDatosValidos_CreaaSolicitudEnPending()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var fechaInicio = _fechaActual.AddDays(5);
        var fechaFin = _fechaActual.AddDays(10);
        var rango = RangoFechas.Crear(fechaInicio, fechaFin, _fechaActual);

        // Act
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones familiares", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(empleadoId, solicitud.EmpleadoId);
        Assert.Equal(fechaInicio, solicitud.FechaInicio);
        Assert.Equal(fechaFin, solicitud.FechaFin);
        Assert.Equal(EstadoSolicitud.Pending, solicitud.Estado);
        Assert.Equal("Vacaciones familiares", solicitud.Motivo);
    }

    [Fact]
    public void Aprobar_SolicitudPendiente_CambiaEstadoAAprobada()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act
        solicitud.Aprobar(aprobadorId, _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(EstadoSolicitud.Approved, solicitud.Estado);
        Assert.Equal(aprobadorId, solicitud.AprobadoPor);
    }

    [Fact]
    public void Aprobar_MismoEmpleadoQueAprueba_LanzaAutoAprobacionNoPermitidaException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act & Assert
        Assert.Throws<AutoAprobacionNoPermitidaException>(() => 
            solicitud.Aprobar(empleadoId, _fechaActual.ToDateTime(TimeOnly.MinValue)));
    }

    [Fact]
    public void Rechazar_SinComentario_LanzaArgumentException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            solicitud.Rechazar(aprobadorId, "", _fechaActual.ToDateTime(TimeOnly.MinValue)));
    }

    [Fact]
    public void Rechazar_ConComentario_CambiaEstadoARejected()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act
        solicitud.Rechazar(aprobadorId, "No hay disponibilidad", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(EstadoSolicitud.Rejected, solicitud.Estado);
        Assert.Equal("No hay disponibilidad", solicitud.ComentarioAprobador);
    }

    [Fact]
    public void Cancelar_SolicitudPendiente_CambiaEstadoACancelled()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act
        solicitud.Cancelar(_fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(EstadoSolicitud.Cancelled, solicitud.Estado);
    }

    [Fact]
    public void Cancelar_SolicitudAprobada_CambiaEstadoACancelled()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));
        solicitud.Aprobar(aprobadorId, _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act
        solicitud.Cancelar(_fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(EstadoSolicitud.Cancelled, solicitud.Estado);
    }

    [Fact]
    public void Rechazar_SolicitudAprobada_LanzaTransicionEstadoInvalidaException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));
        solicitud.Aprobar(aprobadorId, _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act & Assert
        Assert.Throws<TransicionEstadoInvalidaException>(() => 
            solicitud.Rechazar(aprobadorId, "Comentario", _fechaActual.ToDateTime(TimeOnly.MinValue)));
    }

    [Fact]
    public void CancelarAprobada_ConPeriodoYaIniciado_LanzaCancelacionNoPermitidaException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));
        solicitud.Aprobar(aprobadorId, _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act & Assert - fechaInicio (fechaActual+5) <= fechaActual (el periodo ya inició)
        Assert.Throws<CancelacionNoPermitidaException>(() => 
            solicitud.CancelarAprobada(aprobadorId, _fechaActual.AddDays(6), _fechaActual.ToDateTime(TimeOnly.MinValue)));
    }

    [Fact]
    public void CancelarAprobada_ConPeriodoFuturo_CambiaEstadoACancelled()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));
        solicitud.Aprobar(aprobadorId, _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act - fechaActualDate (fechaActual+2) < fechaInicio (fechaActual+5)
        solicitud.CancelarAprobada(aprobadorId, _fechaActual.AddDays(2), _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(EstadoSolicitud.Cancelled, solicitud.Estado);
    }

    [Fact]
    public void Expirar_SolicitudPendiente_CambiaEstadoAExpired()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act
        solicitud.Expirar(_fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(EstadoSolicitud.Expired, solicitud.Estado);
    }

    [Fact]
    public void Crear_ConMotivoVacio_CreaSolicitudConMotivoVacio()
    {
        // Arrange
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);

        // Act
        var solicitud = SolicitudVacaciones.Crear(Guid.NewGuid(), rango, "", _fechaActual.ToDateTime(TimeOnly.MinValue));
        var solicitudEspacios = SolicitudVacaciones.Crear(Guid.NewGuid(), rango, "   ", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(string.Empty, solicitud.Motivo);
        Assert.Equal(string.Empty, solicitudEspacios.Motivo);
    }

    [Fact]
    public void Crear_CalculaDiasHabilesRequeridos()
    {
        // Arrange - lunes a viernes = 5 días hábiles
        var lunes = GetProximoLunes(_fechaActual);
        var rango = RangoFechas.Crear(lunes, lunes.AddDays(4), _fechaActual);

        // Act
        var solicitud = SolicitudVacaciones.Crear(Guid.NewGuid(), rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(5, solicitud.DiasRequeridos);
    }

    [Fact]
    public void Editar_SolicitudPendiente_CambiaFechasMotivoYDias()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var rangoOriginal = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rangoOriginal, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));
        var nuevoRango = RangoFechas.Crear(_fechaActual.AddDays(8), _fechaActual.AddDays(15), _fechaActual);

        // Act
        solicitud.Editar(nuevoRango, "Vacaciones familiares", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(nuevoRango.FechaInicio, solicitud.FechaInicio);
        Assert.Equal(nuevoRango.FechaFin, solicitud.FechaFin);
        Assert.Equal("Vacaciones familiares", solicitud.Motivo);
        Assert.Equal(nuevoRango.CalcularDiasHabiles(), solicitud.DiasRequeridos);
        Assert.Equal(EstadoSolicitud.Pending, solicitud.Estado);
    }

    [Fact]
    public void Editar_SolicitudAprobada_LanzaInvalidOperationException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));
        solicitud.Aprobar(aprobadorId, _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            solicitud.Editar(rango, "Nuevo motivo", _fechaActual.ToDateTime(TimeOnly.MinValue)));
    }

    [Fact]
    public void Editar_ConMotivoVacio_ActualizaMotivoVacio()
    {
        // Arrange
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(Guid.NewGuid(), rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));
        var nuevoRango = RangoFechas.Crear(_fechaActual.AddDays(8), _fechaActual.AddDays(15), _fechaActual);

        // Act
        solicitud.Editar(nuevoRango, "", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.Equal(string.Empty, solicitud.Motivo);
        Assert.Equal(nuevoRango.FechaInicio, solicitud.FechaInicio);
        Assert.Equal(nuevoRango.FechaFin, solicitud.FechaFin);
    }

    [Fact]
    public void Rechazar_ConComentarioMayorA500Caracteres_LanzaArgumentException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            solicitud.Rechazar(aprobadorId, new string('a', 501), _fechaActual.ToDateTime(TimeOnly.MinValue)));
    }

    [Fact]
    public void Expirar_SolicitudAprobada_LanzaTransicionEstadoInvalidaException()
    {
        // Arrange
        var empleadoId = Guid.NewGuid();
        var aprobadorId = Guid.NewGuid();
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var solicitud = SolicitudVacaciones.Crear(empleadoId, rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));
        solicitud.Aprobar(aprobadorId, _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Act & Assert
        Assert.Throws<TransicionEstadoInvalidaException>(() =>
            solicitud.Expirar(_fechaActual.ToDateTime(TimeOnly.MinValue)));
    }

    [Fact]
    public void EstaEnEstadoFinal_RetornaVerdaderoParaEstadosFinales()
    {
        // Arrange
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), _fechaActual);
        var pendiente = SolicitudVacaciones.Crear(Guid.NewGuid(), rango, "Vacaciones", _fechaActual.ToDateTime(TimeOnly.MinValue));

        // Assert
        Assert.False(pendiente.EstaEnEstadoFinal());
        Assert.True(pendiente.PuedeSerCanceladaPorEmpleado());
        Assert.True(pendiente.PuedeSerEditada());

        pendiente.Cancelar(_fechaActual.ToDateTime(TimeOnly.MinValue));

        Assert.True(pendiente.EstaEnEstadoFinal());
        Assert.False(pendiente.PuedeSerCanceladaPorEmpleado());
        Assert.False(pendiente.PuedeSerEditada());
    }

    private static DateOnly GetProximoLunes(DateOnly fecha)
    {
        var diasHastaLunes = ((int)DayOfWeek.Monday - (int)fecha.DayOfWeek + 7) % 7;
        if (diasHastaLunes == 0) diasHastaLunes = 7;
        return fecha.AddDays(diasHastaLunes);
    }
}
