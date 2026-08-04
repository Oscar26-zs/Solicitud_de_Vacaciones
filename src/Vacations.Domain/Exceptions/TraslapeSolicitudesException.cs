namespace Vacations.Domain.Exceptions;

public sealed class TraslapeSolicitudesException : DomainException
{
    public TraslapeSolicitudesException()
        : base("La solicitud incluye días que ya están comprometidos en otra solicitud.")
    {
    }
}
