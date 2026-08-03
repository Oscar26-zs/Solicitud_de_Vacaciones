using Vacations.Domain.ValueObjects;

namespace Vacations.Domain.Tests.ValueObjects;

public class RangoFechasTests
{
    private static readonly DateTime Hoy = new(2026, 8, 3); // lunes

    [Fact]
    public void FechaInicioAnteriorAManana_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RangoFechas.Crear(Hoy, Hoy.AddDays(3), Hoy));
    }

    [Fact]
    public void FechaFinAnteriorAInicio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RangoFechas.Crear(Hoy.AddDays(5), Hoy.AddDays(3), Hoy));
    }

    [Fact]
    public void RangoMayorDeDosMeses_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RangoFechas.Crear(Hoy.AddMonths(2).AddDays(5), Hoy.AddMonths(2).AddDays(8), Hoy));
    }

    [Theory]
    [InlineData("2026-08-10", "2026-08-14", 5)]   // lun-vie
    [InlineData("2026-08-14", "2026-08-17", 2)]   // vie-lun: excluye sab/dom
    [InlineData("2026-08-07", "2026-08-08", 1)]   // vie-sab: 1 día hábil
    public void CalcularDiasHabiles_ExcluyeSabadosYDomigos(string inicio, string fin, int esperado)
    {
        var rango = RangoFechas.Crear(DateTime.Parse(inicio), DateTime.Parse(fin), Hoy);

        var dias = rango.CalcularDiasHabiles();

        Assert.Equal(esperado, dias);
    }

    [Fact]
    public void RangoValido_CreaCorrectamente()
    {
        var rango = RangoFechas.Crear(Hoy.AddDays(5), Hoy.AddDays(7), Hoy);

        Assert.Equal(Hoy.AddDays(5), rango.FechaInicio);
        Assert.Equal(Hoy.AddDays(7), rango.FechaFin);
    }
}