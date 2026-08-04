using Microsoft.EntityFrameworkCore;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;

namespace Vacations.Infrastructure.Persistence.Repositories;

public class RepositorioSolicitudVacaciones : IRepositorioSolicitudVacaciones
{
    private readonly VacacionesDbContext _context;

    public RepositorioSolicitudVacaciones(VacacionesDbContext context)
    {
        _context = context;
    }

    public async Task<SolicitudVacaciones?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SolicitudesVacaciones
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPorEmpleadoAsync(
        Guid empleadoId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.SolicitudesVacaciones
            .Where(s => s.EmpleadoId == empleadoId)
            .OrderByDescending(s => s.CreadoEn)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPendientesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SolicitudesVacaciones
            .Where(s => s.Estado == EstadoSolicitud.Pending)
            .OrderBy(s => s.CreadoEn)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPendientesExpirablesAsync(
        DateOnly fechaActual, 
        CancellationToken cancellationToken = default)
    {
        return await _context.SolicitudesVacaciones
            .Where(s => s.Estado == EstadoSolicitud.Pending && s.FechaInicio <= fechaActual)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteTraslapeAsync(
        Guid empleadoId, 
        DateOnly fechaInicio, 
        DateOnly fechaFin, 
        Guid? excluirSolicitudId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SolicitudesVacaciones
            .Where(s => s.EmpleadoId == empleadoId)
            .Where(s => s.Estado == EstadoSolicitud.Pending || s.Estado == EstadoSolicitud.Approved)
            .Where(s => s.FechaInicio <= fechaFin && s.FechaFin >= fechaInicio);

        if (excluirSolicitudId.HasValue)
        {
            query = query.Where(s => s.Id != excluirSolicitudId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SolicitudVacaciones>> ObtenerTraslapesAsync(
        Guid empleadoId,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        Guid? excluirSolicitudId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SolicitudesVacaciones
            .Where(s => s.EmpleadoId == empleadoId)
            .Where(s => s.Estado == EstadoSolicitud.Pending || s.Estado == EstadoSolicitud.Approved)
            .Where(s => s.FechaInicio <= fechaFin && s.FechaFin >= fechaInicio);

        if (excluirSolicitudId.HasValue)
        {
            query = query.Where(s => s.Id != excluirSolicitudId.Value);
        }

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(SolicitudVacaciones solicitud, CancellationToken cancellationToken = default)
    {
        await _context.SolicitudesVacaciones.AddAsync(solicitud, cancellationToken);
    }

    public void Actualizar(SolicitudVacaciones solicitud)
    {
        _context.SolicitudesVacaciones.Update(solicitud);
    }

    public async Task<(IReadOnlyList<SolicitudVacaciones> Solicitudes, int TotalCount)> ObtenerPorEmpleadoPaginadoAsync(
        Guid empleadoId,
        EstadoSolicitud? estado,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SolicitudesVacaciones
            .Where(s => s.EmpleadoId == empleadoId);

        if (estado.HasValue)
        {
            query = query.Where(s => s.Estado == estado.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var solicitudes = await query
            .OrderByDescending(s => s.CreadoEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (solicitudes, totalCount);
    }

    public async Task<(IReadOnlyList<SolicitudVacaciones> Solicitudes, int TotalCount)> ObtenerBandejaAprobadorAsync(
        Guid aprobadorId,
        string? filtroEmpleado,
        EstadoSolicitud? estado,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SolicitudesVacaciones
            .Where(s => s.EmpleadoId != aprobadorId);

        if (estado.HasValue)
        {
            query = query.Where(s => s.Estado == estado.Value);
        }

        if (fechaDesde.HasValue)
        {
            query = query.Where(s => s.FechaInicio >= fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            query = query.Where(s => s.FechaFin <= fechaHasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtroEmpleado))
        {
            var termino = filtroEmpleado.Trim();
            var empleadosIds = _context.Empleados
                .Where(e => e.NombreCompleto.Contains(termino) || e.Email.Contains(termino))
                .Select(e => e.Id);
            query = query.Where(s => empleadosIds.Contains(s.EmpleadoId));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var solicitudes = await query
            .OrderBy(s => s.CreadoEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (solicitudes, totalCount);
    }

    public async Task<(int Pendientes, int Aprobadas, int Rechazadas, int DiasAprobados)> ObtenerEstadisticasBandejaAprobadorAsync(
        Guid aprobadorId,
        CancellationToken cancellationToken = default)
    {
        var solicitudes = await _context.SolicitudesVacaciones
            .Where(s => s.EmpleadoId != aprobadorId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var pendientes = solicitudes.Count(s => s.Estado == EstadoSolicitud.Pending);
        var aprobadas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Approved);
        var rechazadas = solicitudes.Count(s => s.Estado == EstadoSolicitud.Rejected);
        var diasAprobados = solicitudes
            .Where(s => s.Estado == EstadoSolicitud.Approved)
            .Sum(s => s.DiasRequeridos);

        return (pendientes, aprobadas, rechazadas, diasAprobados);
    }

    public async Task<(IReadOnlyList<SolicitudVacaciones> Solicitudes, int TotalCount)> ObtenerParaRRHHAsync(
        Guid? empleadoId,
        EstadoSolicitud? estado,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SolicitudesVacaciones.AsQueryable();

        if (empleadoId.HasValue)
        {
            query = query.Where(s => s.EmpleadoId == empleadoId.Value);
        }

        if (estado.HasValue)
        {
            query = query.Where(s => s.Estado == estado.Value);
        }

        if (fechaDesde.HasValue)
        {
            query = query.Where(s => s.FechaInicio >= fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            query = query.Where(s => s.FechaFin <= fechaHasta.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var solicitudes = await query
            .OrderByDescending(s => s.CreadoEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (solicitudes, totalCount);
    }
}
