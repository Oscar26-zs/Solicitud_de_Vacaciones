namespace Vacations.Domain.Exceptions;

public sealed class AprobadorInactivoException : DomainException
{
    public AprobadorInactivoException()
        : base("Un aprobador inactivo no puede aprobar o rechazar solicitudes.")
    {
    }
}
