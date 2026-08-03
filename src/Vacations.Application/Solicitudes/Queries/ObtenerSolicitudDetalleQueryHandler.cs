using Vacations.Application.Abstractions;
using Vacations.Application.Common;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Queries;

/// <summary>
/// Handler del caso de uso CU-05: consulta el detalle de una solicitud con su
/// historial, verificando que el acceso sea válido (dueño, aprobador o RRHH).
/// </summary>
public sealed class ObtenerSolicitudDetalleQueryHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;
    private readonly IUsuarioActual _usuario;

    public ObtenerSolicitudDetalleQueryHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IUsuarioActual usuario)
    {
        _solicitudes = solicitudes;
        _usuario = usuario;
    }

    public async Task<SolicitudDetalleResult> HandleAsync(
        ObtenerSolicitudDetalleQuery query,
        CancellationToken cancellationToken = default)
    {
        var solicitud = await _solicitudes.ObtenerPorIdAsync(query.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException();

        if (solicitud.EmpleadoId != query.EmpleadoSolicitanteId && !TieneAccesoAmplio())
        {
            throw new AccesoNoPermitidoException("No tiene acceso a esta solicitud.");
        }

        var historial = await _solicitudes.ObtenerHistorialAsync(solicitud.Id, cancellationToken);

        return new SolicitudDetalleResult
        {
            Solicitud = new SolicitudDto
            {
                Id = solicitud.Id,
                EmpleadoId = solicitud.EmpleadoId,
                FechaInicio = solicitud.FechaInicio,
                FechaFin = solicitud.FechaFin,
                Dias = solicitud.DiasRequeridos,
                Estado = solicitud.Estado.ToString(),
                Motivo = solicitud.Motivo,
                ComentarioAprobador = solicitud.ComentarioAprobador,
            },
            Historial = historial
                .Select(h => new HistorialEventoDto
                {
                    TipoEvento = h.TipoEvento,
                    EstadoAnterior = h.EstadoAnterior?.ToString(),
                    EstadoNuevo = h.EstadoNuevo?.ToString(),
                    CamposModificados = h.CamposModificados,
                    Actor = h.Actor,
                    Timestamp = h.Timestamp,
                    Comentario = h.Comentario,
                })
                .ToList(),
        };
    }

    private bool TieneAccesoAmplio()
        => _usuario.Roles.Contains("Aprobador", StringComparer.OrdinalIgnoreCase)
           || _usuario.Roles.Contains("RRHH", StringComparer.OrdinalIgnoreCase);
}

public sealed record SolicitudDetalleResult
{
    public SolicitudDto Solicitud { get; init; } = default!;

    public IReadOnlyList<HistorialEventoDto> Historial { get; init; } = Array.Empty<HistorialEventoDto>();
}