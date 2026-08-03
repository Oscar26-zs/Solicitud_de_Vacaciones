using Vacations.Application.Common;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Enums;

namespace Vacations.Application.Solicitudes.Queries;

/// <summary>
/// Handler del caso de uso CU-10: lista solicitudes Pending de otros empleados,
/// con filtros opcionales, paginación y datos de saldo/traslape para decisión.
/// </summary>
public sealed class ObtenerBandejaAprobadorQueryHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;
    private readonly IRepositorioSaldoEmpleado _saldos;

    public ObtenerBandejaAprobadorQueryHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioSaldoEmpleado saldos)
    {
        _solicitudes = solicitudes;
        _saldos = saldos;
    }

    public async Task<PagedResult<SolicitudBandejaItem>> HandleAsync(
        ObtenerBandejaAprobadorQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = NormalizarPageSize(query.PageSize);

        var pendientes = await _solicitudes.ObtenerPendientesAsync(cancellationToken);

        var filtradas = pendientes
            .Where(s =>
                s.EmpleadoId != query.AprobadorEmpleadoId
                && (query.EmpleadoId is null || s.EmpleadoId == query.EmpleadoId)
                && (query.FechaInicio is null || s.FechaInicio >= query.FechaInicio)
                && (query.FechaFin is null || s.FechaFin <= query.FechaFin)
                && (query.Dias is null || s.DiasRequeridos == query.Dias))
            .OrderBy(s => s.FechaInicio)
            .ToList();

        var total = filtradas.Count;
        var items = filtradas
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var resultado = new List<SolicitudBandejaItem>(items.Count);
        foreach (var solicitud in items)
        {
            var saldo = await _saldos.ObtenerPorEmpleadoIdAsync(solicitud.EmpleadoId, cancellationToken);
            var hayTraslape = await _solicitudes.ExisteTraslapeAsync(
                solicitud.EmpleadoId,
                solicitud.FechaInicio,
                solicitud.FechaFin,
                solicitud.Id,
                cancellationToken);

            resultado.Add(new SolicitudBandejaItem
            {
                Solicitud = MapSolicitud(solicitud),
                SaldoDisponible = saldo?.SaldoDisponible ?? 0,
                TieneTraslape = hayTraslape,
            });
        }

        return new PagedResult<SolicitudBandejaItem>
        {
            Items = resultado,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize,
        };
    }

    private static SolicitudDto MapSolicitud(Domain.Entities.SolicitudVacaciones s)
        => new()
        {
            Id = s.Id,
            EmpleadoId = s.EmpleadoId,
            FechaInicio = s.FechaInicio,
            FechaFin = s.FechaFin,
            Dias = s.DiasRequeridos,
            Estado = s.Estado.ToString(),
            Motivo = s.Motivo,
            ComentarioAprobador = s.ComentarioAprobador,
        };

    private static int NormalizarPageSize(int pageSize)
        => PagedResult<SolicitudBandejaItem>.AvailablePageSizes.Contains(pageSize)
            ? pageSize
            : 10;
}

public sealed record SolicitudBandejaItem
{
    public SolicitudDto Solicitud { get; init; } = default!;

    public int SaldoDisponible { get; init; }

    public bool TieneTraslape { get; init; }
}