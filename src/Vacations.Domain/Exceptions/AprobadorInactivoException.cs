namespace Vacations.Domain.Exceptions;

/// <summary>
/// Se lanza cuando un aprobador inactivo intenta aprobar o rechazar
/// (RN-33, RF-024, RF-045).
/// </summary>
public sealed class AprobadorInactivoException : DomainException
{
    public AprobadorInactivoException(string message = "Aprobador inactivo")
        : base(message)
    {
    }
}