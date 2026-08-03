namespace Vacations.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta ejecutar una transición de estado no válida
/// sobre una solicitud (constitution §2, invariante 5).
/// </summary>
public sealed class TransicionEstadoInvalidaException : DomainException
{
    public TransicionEstadoInvalidaException(string message)
        : base(message)
    {
    }
}