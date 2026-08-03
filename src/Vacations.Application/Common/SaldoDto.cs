namespace Vacations.Application.Common;

/// <summary>DTO con el saldo de vacaciones de un empleado.</summary>
public sealed record SaldoDto
{
    public Guid EmpleadoId { get; init; }

    public int Acumulado { get; init; }

    public int Consumido { get; init; }

    public int Pendiente { get; init; }

    public int Disponible { get; init; }
}