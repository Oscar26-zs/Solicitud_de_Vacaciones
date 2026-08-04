namespace Vacations.Domain.Exceptions;

public sealed class AccesoNoAutorizadoException : DomainException
{
    public AccesoNoAutorizadoException()
        : base("No tiene permiso para realizar esta acción.")
    {
    }

    public AccesoNoAutorizadoException(string mensaje)
        : base(mensaje)
    {
    }
}
