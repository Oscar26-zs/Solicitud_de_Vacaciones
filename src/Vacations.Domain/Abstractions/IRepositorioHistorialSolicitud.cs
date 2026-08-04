using Vacations.Domain.Entities;

namespace Vacations.Domain.Abstractions;

public interface IRepositorioHistorialSolicitud
{
    Task<IReadOnlyList<HistorialSolicitud>> ObtenerPorSolicitudIdAsync(
        Guid solicitudId, 
        CancellationToken cancellationToken = default);

    Task AgregarAsync(HistorialSolicitud historial, CancellationToken cancellationToken = default);
}
