namespace Vacations.Domain.Exceptions;

/// <summary>Se lanza cuando el usuario no tiene permiso para realizar/consultar una operación.</summary>
public sealed class AccesoNoPermitidoException : DomainException
{
    public AccesoNoPermitidoException(string message) : base(message)
    {
    }
}