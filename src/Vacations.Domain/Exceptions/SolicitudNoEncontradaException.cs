namespace Vacations.Domain.Exceptions;

public sealed class SolicitudNoEncontradaException : DomainException
{
    public Guid SolicitudId { get; }

    public SolicitudNoEncontradaException(Guid solicitudId)
        : base($"La solicitud con Id '{solicitudId}' no fue encontrada.")
    {
        SolicitudId = solicitudId;
    }
}
