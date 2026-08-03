using Vacations.Domain.Entities;

namespace Vacations.Domain.Abstractions;

/// <summary>
/// Repositorio para la entidad <see cref="SolicitudVacaciones"/>.
/// </summary>
public interface IRepositorioSolicitudVacaciones
{
    Task<SolicitudVacaciones?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPorEmpleadoAsync(Guid empleadoId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SolicitudVacaciones>> ObtenerPendientesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SolicitudVacaciones>> ObtenerTodasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe traslape del rango con solicitudes Pending o Approved
    /// del mismo empleado, excluyendo opcionalmente una solicitud (para edición).
    /// </summary>
    Task<bool> ExisteTraslapeAsync(Guid empleadoId, DateTime inicio, DateTime fin, Guid? excluirSolicitudId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HistorialSolicitud>> ObtenerHistorialAsync(Guid solicitudId, CancellationToken cancellationToken = default);

    Task AgregarAsync(SolicitudVacaciones solicitud, CancellationToken cancellationToken = default);

    Task ActualizarAsync(SolicitudVacaciones solicitud, CancellationToken cancellationToken = default);
}