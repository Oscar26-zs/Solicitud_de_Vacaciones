namespace Vacations.Application.Common;

/// <summary>DTO de un evento del historial de auditoría de una solicitud.</summary>
public sealed record HistorialEventoDto
{
    public string TipoEvento { get; init; } = default!;

    public string? EstadoAnterior { get; init; }

    public string? EstadoNuevo { get; init; }

    public string? CamposModificados { get; init; }

    public string Actor { get; init; } = default!;

    public DateTime Timestamp { get; init; }

    public string? Comentario { get; init; }
}