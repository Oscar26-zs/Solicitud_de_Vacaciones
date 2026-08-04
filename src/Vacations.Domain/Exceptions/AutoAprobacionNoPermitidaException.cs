namespace Vacations.Domain.Exceptions;

public sealed class AutoAprobacionNoPermitidaException : DomainException
{
    public AutoAprobacionNoPermitidaException()
        : base("Un aprobador no puede aprobar sus propias solicitudes.")
    {
    }
}
