using Vacations.Domain.Enums;

namespace Vacations.Domain.Entities;

public sealed class HistorialSolicitud
{
    public Guid Id { get; private set; }
    public Guid SolicitudId { get; private set; }
    public TipoEvento TipoEvento { get; private set; }
    public EstadoSolicitud? EstadoAnterior { get; private set; }
    public EstadoSolicitud? EstadoNuevo { get; private set; }
    public string? CamposModificados { get; private set; }
    public string Actor { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }
    public string? Comentario { get; private set; }

    private HistorialSolicitud()
    {
    }

    public static HistorialSolicitud Crear(
        Guid solicitudId,
        TipoEvento tipoEvento,
        EstadoSolicitud? estadoAnterior,
        EstadoSolicitud? estadoNuevo,
        string actor,
        DateTime timestamp,
        string? comentario = null,
        string? camposModificados = null)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new ArgumentException("El actor no puede estar vacío.", nameof(actor));
        }

        return new HistorialSolicitud
        {
            Id = Guid.NewGuid(),
            SolicitudId = solicitudId,
            TipoEvento = tipoEvento,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            CamposModificados = camposModificados,
            Actor = actor,
            Timestamp = timestamp,
            Comentario = comentario
        };
    }

    public static HistorialSolicitud CrearParaCreacion(
        Guid solicitudId,
        string actor,
        DateTime timestamp)
    {
        return Crear(
            solicitudId,
            TipoEvento.Created,
            null,
            EstadoSolicitud.Pending,
            actor,
            timestamp);
    }

    public static HistorialSolicitud CrearParaCambioEstado(
        Guid solicitudId,
        EstadoSolicitud estadoAnterior,
        EstadoSolicitud estadoNuevo,
        string actor,
        DateTime timestamp,
        string? comentario = null)
    {
        return Crear(
            solicitudId,
            TipoEvento.StatusChanged,
            estadoAnterior,
            estadoNuevo,
            actor,
            timestamp,
            comentario);
    }

    public static HistorialSolicitud CrearParaEdicion(
        Guid solicitudId,
        string actor,
        DateTime timestamp,
        string camposModificados)
    {
        return Crear(
            solicitudId,
            TipoEvento.Updated,
            EstadoSolicitud.Pending,
            EstadoSolicitud.Pending,
            actor,
            timestamp,
            camposModificados: camposModificados);
    }
}
