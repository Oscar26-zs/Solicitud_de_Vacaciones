using Vacations.Domain.Abstractions;

namespace Vacations.Application.Solicitudes.Queries;

public sealed class ObtenerBandejaAprobadorQueryHandler
{
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;
    private readonly IRepositorioEmpleado _repositorioEmpleados;
    private readonly IRepositorioSaldoEmpleado _repositorioSaldos;

    private static readonly int[] TamanosPaginaValidos = [5, 10, 15, 25];

    public ObtenerBandejaAprobadorQueryHandler(
        IRepositorioSolicitudVacaciones repositorioSolicitudes,
        IRepositorioEmpleado repositorioEmpleados,
        IRepositorioSaldoEmpleado repositorioSaldos)
    {
        _repositorioSolicitudes = repositorioSolicitudes;
        _repositorioEmpleados = repositorioEmpleados;
        _repositorioSaldos = repositorioSaldos;
    }

    public async Task<BandejaAprobadorResultado> HandleAsync(
        ObtenerBandejaAprobadorQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageSize = TamanosPaginaValidos.Contains(query.PageSize) ? query.PageSize : 10;
        var page = query.Page < 1 ? 1 : query.Page;

        var (solicitudes, totalCount) = await _repositorioSolicitudes.ObtenerBandejaAprobadorAsync(
            query.AprobadorId,
            query.FiltroEmpleado,
            query.Estado,
            query.FechaDesde,
            query.FechaHasta,
            page,
            pageSize,
            cancellationToken);

        var (pendientes, aprobadas, rechazadas, diasAprobados) =
            await _repositorioSolicitudes.ObtenerEstadisticasBandejaAprobadorAsync(query.AprobadorId, cancellationToken);

        var empleadosActivos = await _repositorioEmpleados.ObtenerActivosAsync(cancellationToken);

        var dtos = new List<BandejaSolicitudDto>();

        foreach (var solicitud in solicitudes)
        {
            var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(solicitud.EmpleadoId, cancellationToken);
            var saldo = await _repositorioSaldos.ObtenerPorEmpleadoIdAsync(solicitud.EmpleadoId, cancellationToken);

            dtos.Add(new BandejaSolicitudDto(
                solicitud.Id,
                solicitud.EmpleadoId,
                empleado?.NombreCompleto ?? "Desconocido",
                empleado?.Email ?? "Desconocido",
                solicitud.FechaInicio,
                solicitud.FechaFin,
                solicitud.DiasRequeridos,
                solicitud.Motivo,
                solicitud.CreadoEn,
                solicitud.Estado,
                saldo?.SaldoDisponible ?? 0));
        }

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var estadisticas = new BandejaAprobadorEstadisticas(
            pendientes,
            aprobadas,
            rechazadas,
            empleadosActivos.Count,
            diasAprobados);

        return new BandejaAprobadorResultado(dtos, totalCount, page, pageSize, totalPages, estadisticas);
    }
}
