namespace Vacations.Application.Solicitudes.Queries;

public sealed record ObtenerSolicitudDetalleQuery(
    Guid SolicitudId,
    Guid UsuarioId,
    bool EsAprobador,
    bool EsRRHH);
