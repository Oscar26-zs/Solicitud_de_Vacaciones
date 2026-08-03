namespace Vacations.Application.Common;

/// <summary>DTO de un empleado para transferencia entre capas.</summary>
public sealed record EmpleadoDto
{
    public Guid Id { get; init; }

    public string Email { get; init; } = default!;

    public string NombreCompleto { get; init; } = default!;

    public DateTime FechaIngreso { get; init; }

    public bool EstaActivo { get; init; }
}