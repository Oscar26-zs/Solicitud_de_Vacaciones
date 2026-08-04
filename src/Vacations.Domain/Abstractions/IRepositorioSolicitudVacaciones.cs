using Vacations.Domain.Entities;
using Vacations.Domain.Enums;

namespace Vacations.Domain.Abstractions;

public interface IRepositorioSolicitudVacaciones
{
    Task<SolicitudVacaciones?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPorEmpleadoAsync(
        Guid empleadoId, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPendientesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPendientesExpirablesAsync(
        DateOnly fechaActual, 
        CancellationToken cancellationToken = default);

    Task<bool> ExisteTraslapeAsync(
        Guid empleadoId, 
        DateOnly fechaInicio, 
        DateOnly fechaFin, 
        Guid? excluirSolicitudId = null,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(SolicitudVacaciones solicitud, CancellationToken cancellationToken = default);

    void Actualizar(SolicitudVacaciones solicitud);

    Task<(IReadOnlyList<SolicitudVacaciones> Solicitudes, int TotalCount)> ObtenerPorEmpleadoPaginadoAsync(
        Guid empleadoId,
        EstadoSolicitud? estado,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<SolicitudVacaciones> Solicitudes, int TotalCount)> ObtenerBandejaAprobadorAsync(
        Guid aprobadorId,
        string? filtroEmpleado,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<SolicitudVacaciones> Solicitudes, int TotalCount)> ObtenerParaRRHHAsync(
        Guid? empleadoId,
        EstadoSolicitud? estado,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
