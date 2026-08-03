namespace Vacations.Domain.Exceptions;

/// <summary>Se lanza cuando no existe un saldo registrado para el empleado.</summary>
public sealed class SaldoNoEncontradoException : DomainException
{
    public SaldoNoEncontradoException(string message) : base(message)
    {
    }
}