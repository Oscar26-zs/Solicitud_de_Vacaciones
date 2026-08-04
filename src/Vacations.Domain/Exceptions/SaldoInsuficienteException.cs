namespace Vacations.Domain.Exceptions;

public sealed class SaldoInsuficienteException : DomainException
{
    public int SaldoDisponible { get; }
    public int DiasRequeridos { get; }

    public SaldoInsuficienteException(int saldoDisponible, int diasRequeridos)
        : base($"Saldo insuficiente para esta solicitud. Saldo disponible: {saldoDisponible} días. Días requeridos: {diasRequeridos}.")
    {
        SaldoDisponible = saldoDisponible;
        DiasRequeridos = diasRequeridos;
    }
}
