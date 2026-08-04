using Vacations.Domain.Entities;
using Vacations.Domain.Enums;

namespace Vacations.Domain.Tests.Entities;

public class HistorialSolicitudTests
{
    private readonly DateTime _timestamp = DateTime.UtcNow;

    [Fact]
    public void Crear_ConDatosValidos_CreaHistorial()
    {
        // Arrange
        var solicitudId = Guid.NewGuid();

        // Act
        var historial = HistorialSolicitud.Crear(
            solicitudId,
            TipoEvento.StatusChanged,
            EstadoSolicitud.Pending,
            EstadoSolicitud.Approved,
            "aprobador@example.com",
            _timestamp,
            "Sin comentario",
            "{\"Estado\":\"Approved\"}");

        // Assert
        Assert.NotEqual(Guid.Empty, historial.Id);
        Assert.Equal(solicitudId, historial.SolicitudId);
        Assert.Equal(TipoEvento.StatusChanged, historial.TipoEvento);
        Assert.Equal(EstadoSolicitud.Pending, historial.EstadoAnterior);
        Assert.Equal(EstadoSolicitud.Approved, historial.EstadoNuevo);
        Assert.Equal("aprobador@example.com", historial.Actor);
        Assert.Equal(_timestamp, historial.Timestamp);
        Assert.Equal("Sin comentario", historial.Comentario);
        Assert.Equal("{\"Estado\":\"Approved\"}", historial.CamposModificados);
    }

    [Fact]
    public void Crear_ConActorVacio_LanzaArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HistorialSolicitud.Crear(
            Guid.NewGuid(), TipoEvento.Created, null, EstadoSolicitud.Pending, "", _timestamp));
    }

    [Fact]
    public void Crear_ConActorEspaciosEnBlanco_LanzaArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HistorialSolicitud.Crear(
            Guid.NewGuid(), TipoEvento.Created, null, EstadoSolicitud.Pending, "   ", _timestamp));
    }

    [Fact]
    public void CrearParaCreacion_ConfiguraEventoCreated()
    {
        // Arrange
        var solicitudId = Guid.NewGuid();

        // Act
        var historial = HistorialSolicitud.CrearParaCreacion(solicitudId, "empleado@example.com", _timestamp);

        // Assert
        Assert.Equal(TipoEvento.Created, historial.TipoEvento);
        Assert.Null(historial.EstadoAnterior);
        Assert.Equal(EstadoSolicitud.Pending, historial.EstadoNuevo);
        Assert.Equal("empleado@example.com", historial.Actor);
    }

    [Fact]
    public void CrearParaCambioEstado_ConfiguraEventoStatusChanged()
    {
        // Arrange
        var solicitudId = Guid.NewGuid();

        // Act
        var historial = HistorialSolicitud.CrearParaCambioEstado(
            solicitudId, EstadoSolicitud.Pending, EstadoSolicitud.Rejected, "aprobador@example.com", _timestamp, "Motivo");

        // Assert
        Assert.Equal(TipoEvento.StatusChanged, historial.TipoEvento);
        Assert.Equal(EstadoSolicitud.Pending, historial.EstadoAnterior);
        Assert.Equal(EstadoSolicitud.Rejected, historial.EstadoNuevo);
        Assert.Equal("Motivo", historial.Comentario);
    }

    [Fact]
    public void CrearParaEdicion_ConfiguraEventoUpdatedYCamposModificados()
    {
        // Arrange
        var solicitudId = Guid.NewGuid();

        // Act
        var historial = HistorialSolicitud.CrearParaEdicion(solicitudId, "empleado@example.com", _timestamp, "[\"FechaInicio\",\"Motivo\"]");

        // Assert
        Assert.Equal(TipoEvento.Updated, historial.TipoEvento);
        Assert.Equal(EstadoSolicitud.Pending, historial.EstadoAnterior);
        Assert.Equal(EstadoSolicitud.Pending, historial.EstadoNuevo);
        Assert.Equal("[\"FechaInicio\",\"Motivo\"]", historial.CamposModificados);
    }
}
