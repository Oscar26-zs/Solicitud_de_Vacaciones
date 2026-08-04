using Vacations.Domain.Enums;

namespace Vacations.Domain.Exceptions;

public sealed class TransicionEstadoInvalidaException : DomainException
{
    public EstadoSolicitud EstadoActual { get; }
    public EstadoSolicitud EstadoIntentado { get; }

    public TransicionEstadoInvalidaException(EstadoSolicitud estadoActual, EstadoSolicitud estadoIntentado)
        : base($"Transición de estado inválida: no se puede pasar de '{estadoActual}' a '{estadoIntentado}'.")
    {
        EstadoActual = estadoActual;
        EstadoIntentado = estadoIntentado;
    }
}
