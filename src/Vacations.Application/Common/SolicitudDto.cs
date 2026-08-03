namespace Vacations.Application.Common;

/// <summary>DTO de una solicitud de vacaciones para transferencia entre capas (record inmutable).</summary>
public sealed record SolicitudDto
{
    public Guid Id { get; init; }

    public Guid EmpleadoId { get; init; }

    public DateTime FechaInicio { get; init; }

    public DateTime FechaFin { get; init; }

    public int Dias { get; init; }

    public string Estado { get; init; } = default!;

    public string Motivo { get; init; } = default!;

    public string? ComentarioAprobador { get; init; }
}