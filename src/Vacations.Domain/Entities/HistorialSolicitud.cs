using Vacations.Domain.Enums;

namespace Vacations.Domain.Entities;

/// <summary>
/// Registro de auditoría inmutable para cada acción sobre una solicitud
/// (CU-17, RF-032). Solo se crea; nunca se modifica ni elimina.
/// </summary>
public sealed class HistorialSolicitud
{
    public static class EventoConstante
    {
        public const string Creado = "CREATED";
        public const string Actualizado = "UPDATED";
        public const string EstadoCambiado = "STATUS_CHANGED";
        public const string Cancelado = "CANCELLED";
    }

    public Guid Id { get; }

    public Guid SolicitudId { get; }

    public string TipoEvento { get; }

    public EstadoSolicitud? EstadoAnterior { get; }

    public EstadoSolicitud? EstadoNuevo { get; }

    /// <summary>JSON con los campos modificados en ediciones: <c>{"campo": {"old": "...", "new": "..."}}</c>.</summary>
    public string? CamposModificados { get; }

    public string Actor { get; }

    public DateTime Timestamp { get; }

    public string? Comentario { get; }

    private HistorialSolicitud(
        Guid id,
        Guid solicitudId,
        string tipoEvento,
        EstadoSolicitud? estadoAnterior,
        EstadoSolicitud? estadoNuevo,
        string? camposModificados,
        string actor,
        DateTime timestamp,
        string? comentario)
    {
        Id = id;
        SolicitudId = solicitudId;
        TipoEvento = tipoEvento;
        EstadoAnterior = estadoAnterior;
        EstadoNuevo = estadoNuevo;
        CamposModificados = camposModificados;
        Actor = actor;
        Timestamp = timestamp;
        Comentario = comentario;
    }

    public static HistorialSolicitud Crear(
        Guid solicitudId,
        string tipoEvento,
        string actor,
        DateTime timestamp,
        EstadoSolicitud? estadoAnterior = null,
        EstadoSolicitud? estadoNuevo = null,
        string? camposModificados = null,
        string? comentario = null)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new ArgumentException("El actor es obligatorio", nameof(actor));
        }

        return new HistorialSolicitud(
            Guid.NewGuid(),
            solicitudId,
            tipoEvento,
            estadoAnterior,
            estadoNuevo,
            camposModificados,
            actor.Trim(),
            timestamp,
            comentario);
    }
}