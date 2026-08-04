using Vacations.Application.Solicitudes.Commands;

namespace Vacations.Application.Tests.Solicitudes;

public class RechazarSolicitudCommandValidatorTests
{
    private readonly RechazarSolicitudCommandValidator _validator = new();

    private static RechazarSolicitudCommand ComandoValido() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "No hay disponibilidad");

    [Fact]
    public void Validate_ComandoValido_EsValido()
    {
        // Act
        var result = _validator.Validate(ComandoValido());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_SolicitudIdVacio_EsInvalido()
    {
        // Arrange
        var command = ComandoValido() with { SolicitudId = Guid.Empty };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SolicitudId");
    }

    [Fact]
    public void Validate_AprobadorIdVacio_EsInvalido()
    {
        // Arrange
        var command = ComandoValido() with { AprobadorId = Guid.Empty };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AprobadorId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ComentarioVacio_EsInvalido(string comentario)
    {
        // Arrange
        var command = ComandoValido() with { Comentario = comentario };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Comentario");
    }

    [Fact]
    public void Validate_ComentarioMayorA500_EsInvalido()
    {
        // Arrange
        var command = ComandoValido() with { Comentario = new string('a', 501) };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Comentario");
    }
}