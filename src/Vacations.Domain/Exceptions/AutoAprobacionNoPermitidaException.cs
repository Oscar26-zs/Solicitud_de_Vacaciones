namespace Vacations.Domain.Exceptions;

/// <summary>
/// Se lanza cuando un aprobador intenta aprobar o rechazar su propia solicitud
/// (RN-32, RF-024, RF-044).
/// </summary>
public sealed class AutoAprobacionNoPermitidaException : DomainException
{
    public AutoAprobacionNoPermitidaException(string message = "No puedes aprobar ni rechazar tu propia solicitud; otro aprobador debe resolverla")
        : base(message)
    {
    }
}