namespace Vacations.Domain.Exceptions;

/// <summary>
/// Se lanza cuando el rango solicitado se solapa con solicitudes Approved o
/// Pending del mismo empleado (RN-07, RF-006, RF-021).
/// </summary>
public sealed class TraslapeSolicitudesException : DomainException
{
    public TraslapeSolicitudesException(string message = "La solicitud incluye días que ya están comprometidos en otra solicitud")
        : base(message)
    {
    }
}