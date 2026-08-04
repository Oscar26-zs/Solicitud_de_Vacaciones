namespace Vacations.Application.Saldos.Queries;

public sealed record SaldoDto(
    int SaldoAcumulado,
    int SaldoConsumido,
    int SaldoPendiente,
    int SaldoDisponible,
    DateTime UltimaActualizacion);
