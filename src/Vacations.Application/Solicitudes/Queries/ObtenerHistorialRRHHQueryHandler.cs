using Vacations.Application.Abstractions;
using Vacations.Application.Common;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;

namespace Vacations.Application.Solicitudes.Queries;

/// <summary>
/// Handler del caso de uso CU-18: consulta y filtra solicitudes de cualquier
/// empleado (acceso restringido por política al rol RRHH en la capa Web).
/// </summary>
public sealed class ObtenerHistorialRRHHQueryHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;
    private readonly IRepositorioEmpleado _empleados;
    private readonly IUsuarioActual _usuario;

    public ObtenerHistorialRRHHQueryHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioEmpleado empleados,
        IUsuarioActual usuario)
    {
        _solicitudes = solicitudes;
        _empleados = empleados;
        _usuario = usuario;
    }

    public async Task<PagedResult<SolicitudRRHHItem>> HandleAsync(
        ObtenerHistorialRRHHQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_usuario.Roles.Contains("RRHH", StringComparer.OrdinalIgnoreCase))
        {
            throw new AccesoNoPermitidoException("Solo el rol RRHH puede acceder al historial completo.");
        }

        var page = Math.Max(1, query.Page);
        var pageSize = NormalizarPageSize(query.PageSize);

        var todas = query.EmpleadoId.HasValue
            ? await _solicitudes.ObtenerPorEmpleadoAsync(query.EmpleadoId.Value, cancellationToken)
            : await _solicitudes.ObtenerTodasAsync(cancellationToken);

        var filtradas = todas
            .Where(s =>
                (query.Estado is null || s.Estado == query.Estado)
                && (query.FechaInicio is null || s.FechaInicio >= query.FechaInicio)
                && (query.FechaFin is null || s.FechaFin <= query.FechaFin))
            .OrderByDescending(s => s.CreadoEn)
            .ToList();

        var total = filtradas.Count;
        var items = filtradas
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var empleadoIds = items.Select(s => s.EmpleadoId).Distinct().ToList();
        var empleadosDict = new Dictionary<Guid, Empleado>();
        foreach (var id in empleadoIds)
        {
            var e = await _empleados.ObtenerPorIdAsync(id, cancellationToken);
            if (e is not null)
            {
                empleadosDict[id] = e;
            }
        }

        var resultado = items
            .Select(s => new SolicitudRRHHItem
            {
                Solicitud = new SolicitudDto
                {
                    Id = s.Id,
                    EmpleadoId = s.EmpleadoId,
                    FechaInicio = s.FechaInicio,
                    FechaFin = s.FechaFin,
                    Dias = s.DiasRequeridos,
                    Estado = s.Estado.ToString(),
                    Motivo = s.Motivo,
                    ComentarioAprobador = s.ComentarioAprobador,
                },
                Empleado = empleadosDict.TryGetValue(s.EmpleadoId, out var emp)
                    ? new Common.EmpleadoDto
                    {
                        Id = emp.Id,
                        Email = emp.Email,
                        NombreCompleto = emp.NombreCompleto,
                        FechaIngreso = emp.FechaIngreso,
                        EstaActivo = emp.EstaActivo,
                    }
                    : null,
            })
            .ToList();

        return new PagedResult<SolicitudRRHHItem>
        {
            Items = resultado,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize,
        };
    }

    private static int NormalizarPageSize(int pageSize)
        => PagedResult<SolicitudRRHHItem>.AvailablePageSizes.Contains(pageSize) ? pageSize : 10;
}

public sealed record SolicitudRRHHItem
{
    public SolicitudDto Solicitud { get; init; } = default!;

    public Common.EmpleadoDto? Empleado { get; init; }
}