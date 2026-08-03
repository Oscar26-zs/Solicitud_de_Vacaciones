namespace Vacations.Domain.Exceptions;

/// <summary>
/// Excepción base para todas las excepciones de la capa de dominio.
/// Sirve como tipo común para el manejo global de errores de negocio.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}