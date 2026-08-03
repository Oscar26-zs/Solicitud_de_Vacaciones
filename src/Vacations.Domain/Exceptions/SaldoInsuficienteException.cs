namespace Vacations.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta crear/aprobar una solicitud que excede el
/// saldo disponible del empleado (RN-02, RF-005, RF-025).
/// </summary>
public sealed class SaldoInsuficienteException : DomainException
{
    public SaldoInsuficienteException(string message = "Saldo insuficiente para esta solicitud")
        : base(message)
    {
    }
}