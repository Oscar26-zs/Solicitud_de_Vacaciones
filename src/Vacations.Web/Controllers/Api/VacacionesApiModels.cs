namespace Vacations.Web.Controllers.Api;

/// <summary>
/// Contrato consumido por el agente de IA (proyecto Oro-Agente, Python/FastAPI).
/// EmpleadoId y SolicitudId viajan como Guid (serializados como string en JSON):
/// el dominio usa Guid en todo el sistema, así que el lado Python debe tratarlos
/// como string, no como int. Ver TAREAS.md, discrepancias #1 y #2.
/// </summary>
public sealed record SolicitarVacacionesRequest(
    Guid EmpleadoId,
    string Destino,
    DateOnly FechaInicio,
    DateOnly FechaFin);

public sealed record SolicitarVacacionesResponse(
    Guid SolicitudId,
    string Estado);

public sealed record EstadoSolicitudResponse(
    Guid SolicitudId,
    string Estado);

public sealed record ErrorResponse(string Error);
