using Vacations.Application.Common;
using Vacations.Domain.Abstractions;

namespace Vacations.Application.Solicitudes.Queries;

/// <summary>
/// Handler del caso de uso CU-05: lista las solicitudes del empleado, con filtro
/// opcional por estado y paginación offset-based (5/10/15/25), más reciente primero.
/// </summary>
public sealed class ObtenerMisSolicitudesQueryHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;

    public ObtenerMisSolicitudesQueryHandler(IRepositorioSolicitudVacaciones solicitudes)
    {
        _solicitudes = solicitudes;
    }

    public async Task<PagedResult<SolicitudDto>> HandleAsync(
        ObtenerMisSolicitudesQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = NormalizarPageSize(query.PageSize);

        var todas = await _solicitudes.ObtenerPorEmpleadoAsync(query.EmpleadoId, cancellationToken);

        var filtradas = query.Estado is null
            ? todas
            : todas.Where(s => s.Estado == query.Estado).ToList();

        var total = filtradas.Count;
        var items = filtradas
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SolicitudDto
            {
                Id = s.Id,
                EmpleadoId = s.EmpleadoId,
                FechaInicio = s.FechaInicio,
                FechaFin = s.FechaFin,
                Dias = s.DiasRequeridos,
                Estado = s.Estado.ToString(),
                Motivo = s.Motivo,
                ComentarioAprobador = s.ComentarioAprobador,
            })
            .ToList();

        return new PagedResult<SolicitudDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize,
        };
    }

    private static int NormalizarPageSize(int pageSize)
        => PagedResult<SolicitudDto>.AvailablePageSizes.Contains(pageSize)
            ? pageSize
            : 10;
}