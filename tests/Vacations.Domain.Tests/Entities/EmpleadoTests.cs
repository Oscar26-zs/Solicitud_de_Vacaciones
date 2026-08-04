using Vacations.Domain.Entities;

namespace Vacations.Domain.Tests.Entities;

public class EmpleadoTests
{
    [Fact]
    public void Crear_ConDatosValidos_CreaEmpleado()
    {
        // Arrange
        var email = "test@example.com";
        var nombre = "Juan Pérez";
        var fechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));

        // Act
        var empleado = Empleado.Crear(email, nombre, fechaIngreso);

        // Assert
        Assert.NotEqual(Guid.Empty, empleado.Id);
        Assert.Equal("test@example.com", empleado.Email);
        Assert.Equal(nombre, empleado.NombreCompleto);
        Assert.Equal(fechaIngreso, empleado.FechaIngreso);
        Assert.True(empleado.EstaActivo);
    }

    [Fact]
    public void Crear_ConEmailVacio_LanzaArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Empleado.Crear("", "Juan Pérez", DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    public void Crear_ConNombreVacio_LanzaArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Empleado.Crear("test@example.com", "", DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    public void Crear_NormalizaEmail()
    {
        // Arrange & Act
        var empleado = Empleado.Crear("TEST@EXAMPLE.COM", "Juan Pérez", DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        Assert.Equal("test@example.com", empleado.Email);
    }

    [Fact]
    public void Desactivar_CambiaEstaActivoAFalse()
    {
        // Arrange
        var empleado = Empleado.Crear("test@example.com", "Juan Pérez", DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        empleado.Desactivar();

        // Assert
        Assert.False(empleado.EstaActivo);
    }

    [Fact]
    public void Activar_CambiaEstaActivoATrue()
    {
        // Arrange
        var empleado = Empleado.Crear("test@example.com", "Juan Pérez", DateOnly.FromDateTime(DateTime.UtcNow));
        empleado.Desactivar();

        // Act
        empleado.Activar();

        // Assert
        Assert.True(empleado.EstaActivo);
    }

    [Fact]
    public void ActualizarNombre_CambiaElNombre()
    {
        // Arrange
        var empleado = Empleado.Crear("test@example.com", "Juan Pérez", DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        empleado.ActualizarNombre("Maria García");

        // Assert
        Assert.Equal("Maria García", empleado.NombreCompleto);
    }

    [Fact]
    public void ActualizarNombre_ConNombreVacio_LanzaArgumentException()
    {
        // Arrange
        var empleado = Empleado.Crear("test@example.com", "Juan Pérez", DateOnly.FromDateTime(DateTime.UtcNow));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => empleado.ActualizarNombre(""));
    }
}
