using Vacations.Domain.Abstractions;

namespace Vacations.Application.Solicitudes.Queries;

public sealed class ObtenerMisSolicitudesQueryHandler
{
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;

    private static readonly int[] TamanosPaginaValidos = [5, 10, 15, 25];

    public ObtenerMisSolicitudesQueryHandler(IRepositorioSolicitudVacaciones repositorioSolicitudes)
    {
        _repositorioSolicitudes = repositorioSolicitudes;
    }

    public async Task<ListaSolicitudesResultado> HandleAsync(
        ObtenerMisSolicitudesQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageSize = TamanosPaginaValidos.Contains(query.PageSize) ? query.PageSize : 10;
        var page = query.Page < 1 ? 1 : query.Page;

        var (solicitudes, totalCount) = await _repositorioSolicitudes.ObtenerPorEmpleadoPaginadoAsync(
            query.EmpleadoId,
            query.Estado,
            page,
            pageSize,
            cancellationToken);

        var dtos = solicitudes.Select(s => new SolicitudResumenDto(
            s.Id,
            s.FechaInicio,
            s.FechaFin,
            s.DiasRequeridos,
            s.Estado,
            s.Motivo,
            s.ComentarioAprobador,
            s.CreadoEn)).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new ListaSolicitudesResultado(dtos, totalCount, page, pageSize, totalPages);
    }
}
