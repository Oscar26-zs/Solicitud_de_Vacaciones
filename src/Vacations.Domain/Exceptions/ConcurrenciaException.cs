namespace Vacations.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando ocurre un conflicto de concurrencia optimista.
/// </summary>
public sealed class ConcurrenciaException : DomainException
{
    public ConcurrenciaException(string mensaje = "Conflicto de concurrencia. Los datos fueron modificados por otro usuario. Intente nuevamente.")
        : base(mensaje)
    {
    }
}