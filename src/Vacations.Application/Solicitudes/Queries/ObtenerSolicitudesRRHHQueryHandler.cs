using Vacations.Domain.Abstractions;

namespace Vacations.Application.Solicitudes.Queries;

public sealed class ObtenerSolicitudesRRHHQueryHandler
{
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;
    private readonly IRepositorioEmpleado _repositorioEmpleados;

    private static readonly int[] TamanosPaginaValidos = [5, 10, 15, 25];

    public ObtenerSolicitudesRRHHQueryHandler(
        IRepositorioSolicitudVacaciones repositorioSolicitudes,
        IRepositorioEmpleado repositorioEmpleados)
    {
        _repositorioSolicitudes = repositorioSolicitudes;
        _repositorioEmpleados = repositorioEmpleados;
    }

    public async Task<ListaSolicitudesRRHHResultado> HandleAsync(
        ObtenerSolicitudesRRHHQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageSize = TamanosPaginaValidos.Contains(query.PageSize) ? query.PageSize : 10;
        var page = query.Page < 1 ? 1 : query.Page;

        var (solicitudes, totalCount) = await _repositorioSolicitudes.ObtenerParaRRHHAsync(
            query.EmpleadoId,
            query.Estado,
            query.FechaDesde,
            query.FechaHasta,
            page,
            pageSize,
            cancellationToken);

        var dtos = new List<SolicitudRRHHDto>();

        foreach (var solicitud in solicitudes)
        {
            var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(solicitud.EmpleadoId, cancellationToken);

            dtos.Add(new SolicitudRRHHDto(
                solicitud.Id,
                solicitud.EmpleadoId,
                empleado?.NombreCompleto ?? "Desconocido",
                empleado?.Email ?? "Desconocido",
                solicitud.FechaInicio,
                solicitud.FechaFin,
                solicitud.DiasRequeridos,
                solicitud.Estado,
                solicitud.Motivo,
                solicitud.ComentarioAprobador,
                solicitud.CreadoEn));
        }

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new ListaSolicitudesRRHHResultado(dtos, totalCount, page, pageSize, totalPages);
    }
}
