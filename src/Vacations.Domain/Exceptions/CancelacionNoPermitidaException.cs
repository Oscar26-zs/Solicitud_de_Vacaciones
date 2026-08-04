namespace Vacations.Domain.Exceptions;

public sealed class CancelacionNoPermitidaException : DomainException
{
    public CancelacionNoPermitidaException(string motivo)
        : base($"No se puede cancelar la solicitud: {motivo}")
    {
    }
}
