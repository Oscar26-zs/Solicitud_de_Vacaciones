using Vacations.Domain.ValueObjects;

namespace Vacations.Domain.Tests.ValueObjects;

public class RangoFechasTests
{
    private readonly DateOnly _fechaActual = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Crear_ConDatosValidos_CreaRangoFechas()
    {
        // Arrange
        var fechaInicio = _fechaActual.AddDays(5);
        var fechaFin = _fechaActual.AddDays(10);

        // Act
        var rango = RangoFechas.Crear(fechaInicio, fechaFin, _fechaActual);

        // Assert
        Assert.Equal(fechaInicio, rango.FechaInicio);
        Assert.Equal(fechaFin, rango.FechaFin);
    }

    [Fact]
    public void Crear_FechaInicioAyer_LanzaArgumentException()
    {
        // Arrange
        var fechaInicio = _fechaActual; // Hoy, debe ser mañana como mínimo
        var fechaFin = _fechaActual.AddDays(5);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RangoFechas.Crear(fechaInicio, fechaFin, _fechaActual));
    }

    [Fact]
    public void Crear_FechaFinAntesDeFechaInicio_LanzaArgumentException()
    {
        // Arrange
        var fechaInicio = _fechaActual.AddDays(10);
        var fechaFin = _fechaActual.AddDays(5);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RangoFechas.Crear(fechaInicio, fechaFin, _fechaActual));
    }

    [Fact]
    public void Crear_HorizonteMayorA2Meses_LanzaArgumentException()
    {
        // Arrange
        var fechaInicio = _fechaActual.AddMonths(3);
        var fechaFin = _fechaActual.AddMonths(3).AddDays(5);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => RangoFechas.Crear(fechaInicio, fechaFin, _fechaActual));
    }

    [Fact]
    public void CalcularDiasHabiles_ExcluyeFinesDeSemana()
    {
        // Arrange - Una semana completa (lunes a domingo) = 5 días hábiles
        var lunes = GetProximoLunes(_fechaActual);
        var domingo = lunes.AddDays(6);
        var rango = RangoFechas.Crear(lunes, domingo, _fechaActual.AddDays(-1)); // Usamos fecha anterior para pasar validación

        // Act
        var diasHabiles = rango.CalcularDiasHabiles();

        // Assert - Lunes a Viernes = 5 días hábiles
        Assert.Equal(5, diasHabiles);
    }

    [Fact]
    public void SeSolapa_ConRangoQueSeSolapa_RetornaTrue()
    {
        // Arrange
        var fechaBase = _fechaActual.AddDays(-10); // Para pasar validaciones
        var rango1 = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), fechaBase);
        var rango2 = RangoFechas.Crear(_fechaActual.AddDays(8), _fechaActual.AddDays(15), fechaBase);

        // Act
        var seSolapa = rango1.SeTraslapaCon(rango2);

        // Assert
        Assert.True(seSolapa);
    }

    [Fact]
    public void SeSolapa_ConRangoQueNoSeSolapa_RetornaFalse()
    {
        // Arrange
        var fechaBase = _fechaActual.AddDays(-10);
        var rango1 = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), fechaBase);
        var rango2 = RangoFechas.Crear(_fechaActual.AddDays(15), _fechaActual.AddDays(20), fechaBase);

        // Act
        var seSolapa = rango1.SeTraslapaCon(rango2);

        // Assert
        Assert.False(seSolapa);
    }

    [Fact]
    public void Equals_ConMismasFechas_RetornaTrue()
    {
        // Arrange
        var fechaBase = _fechaActual.AddDays(-10);
        var rango1 = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), fechaBase);
        var rango2 = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), fechaBase);

        // Act & Assert
        Assert.Equal(rango1, rango2);
    }

    [Fact]
    public void CrearSinValidacion_CreaRangoConFechasPasadas()
    {
        // Arrange & Act
        var rango = RangoFechas.CrearSinValidacion(_fechaActual.AddDays(-30), _fechaActual.AddDays(-20));

        // Assert
        Assert.Equal(_fechaActual.AddDays(-30), rango.FechaInicio);
        Assert.Equal(_fechaActual.AddDays(-20), rango.FechaFin);
    }

    [Fact]
    public void OperadoresIgualdad_ComparanPorValor()
    {
        // Arrange
        var fechaBase = _fechaActual.AddDays(-10);
        var rango1 = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), fechaBase);
        var rango2 = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), fechaBase);
        var rango3 = RangoFechas.Crear(_fechaActual.AddDays(6), _fechaActual.AddDays(10), fechaBase);

        // Act & Assert
        Assert.True(rango1 == rango2);
        Assert.False(rango1 == rango3);
        Assert.True(rango1 != rango3);
        Assert.False(rango1 != rango2);
        Assert.Equal(rango1.GetHashCode(), rango2.GetHashCode());
    }

    [Fact]
    public void Equals_ConNull_RetornaFalse()
    {
        // Arrange
        var fechaBase = _fechaActual.AddDays(-10);
        var rango = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), fechaBase);

        // Act & Assert
        Assert.False(rango.Equals(null));
        Assert.False(rango.Equals((object?)null));
    }

    [Fact]
    public void SeTraslapaCon_RangosContiguos_NoSeSolapan()
    {
        // Arrange
        var fechaBase = _fechaActual.AddDays(-10);
        var rango1 = RangoFechas.Crear(_fechaActual.AddDays(5), _fechaActual.AddDays(10), fechaBase);
        var rango2 = RangoFechas.Crear(_fechaActual.AddDays(11), _fechaActual.AddDays(15), fechaBase);

        // Act
        var seSolapa = rango1.SeTraslapaCon(rango2);

        // Assert
        Assert.False(seSolapa);
    }

    [Fact]
    public void CalcularDiasHabiles_RangoQueTocaFinDeSemana_CuentaCorrectamente()
    {
        // Arrange - viernes a lunes = 2 días hábiles (viernes y lunes)
        var viernes = GetProximoLunes(_fechaActual).AddDays(-3);
        var rango = RangoFechas.Crear(viernes, viernes.AddDays(3), _fechaActual.AddDays(-1));

        // Act
        var diasHabiles = rango.CalcularDiasHabiles();

        // Assert
        Assert.Equal(2, diasHabiles);
    }

    private static DateOnly GetProximoLunes(DateOnly fecha)
    {
        var diasHastaLunes = ((int)DayOfWeek.Monday - (int)fecha.DayOfWeek + 7) % 7;
        if (diasHastaLunes == 0) diasHastaLunes = 7;
        return fecha.AddDays(diasHastaLunes);
    }
}
