using Vacations.Application.Solicitudes.Commands;

namespace Vacations.Application.Tests.Solicitudes;

public class CrearSolicitudCommandValidatorTests
{
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly CrearSolicitudCommandValidator _validator;

    public CrearSolicitudCommandValidatorTests()
    {
        _validator = new CrearSolicitudCommandValidator(_timeProvider);
    }

    private static CrearSolicitudCommand ComandoValido() =>
        new(Guid.NewGuid(), new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 14), "Vacaciones familiares");

    [Fact]
    public void Validate_ComandoValido_EsValido()
    {
        // Act
        var result = _validator.Validate(ComandoValido());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmpleadoIdVacio_EsInvalido()
    {
        // Arrange
        var command = ComandoValido() with { EmpleadoId = Guid.Empty };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EmpleadoId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Corto")]
    public void Validate_MotivoInvalido_EsInvalido(string motivo)
    {
        // Arrange
        var command = ComandoValido() with { Motivo = motivo };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Motivo");
    }

    [Fact]
    public void Validate_MotivoMayorA1000_EsInvalido()
    {
        // Arrange
        var command = ComandoValido() with { Motivo = new string('a', 1001) };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Motivo");
    }
}
