namespace Vacations.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta operar sobre una solicitud de vacaciones
/// que no existe en el sistema (404 en la capa web).
/// </summary>
public sealed class SolicitudNoEncontradaException : DomainException
{
    public SolicitudNoEncontradaException(string message = "Solicitud no encontrada")
        : base(message)
    {
    }
}