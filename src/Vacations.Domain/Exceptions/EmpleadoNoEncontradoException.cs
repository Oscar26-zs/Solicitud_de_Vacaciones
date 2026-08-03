namespace Vacations.Domain.Exceptions;

/// <summary>Se lanza cuando no se encuentra un empleado registrado.</summary>
public sealed class EmpleadoNoEncontradoException : DomainException
{
    public EmpleadoNoEncontradoException(string message) : base(message)
    {
    }
}