namespace Vacations.Domain.ValueObjects;

public sealed class RangoFechas : IEquatable<RangoFechas>
{
    public DateOnly FechaInicio { get; }
    public DateOnly FechaFin { get; }

    private RangoFechas(DateOnly fechaInicio, DateOnly fechaFin)
    {
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }

    public static RangoFechas Crear(DateOnly fechaInicio, DateOnly fechaFin, DateOnly fechaActual)
    {
        var manana = fechaActual.AddDays(1);

        if (fechaInicio < manana)
        {
            throw new ArgumentException("La fecha de inicio no puede ser anterior a mañana.", nameof(fechaInicio));
        }

        if (fechaFin < fechaInicio)
        {
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.", nameof(fechaFin));
        }

        var horizonteMaximo = fechaActual.AddMonths(2);
        if (fechaFin > horizonteMaximo)
        {
            throw new ArgumentException("La fecha de fin no puede superar los 2 meses desde la fecha actual.", nameof(fechaFin));
        }

        return new RangoFechas(fechaInicio, fechaFin);
    }

    public static RangoFechas CrearSinValidacion(DateOnly fechaInicio, DateOnly fechaFin)
    {
        return new RangoFechas(fechaInicio, fechaFin);
    }

    public int CalcularDiasHabiles()
    {
        var dias = 0;
        var fecha = FechaInicio;

        while (fecha <= FechaFin)
        {
            if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
            {
                dias++;
            }
            fecha = fecha.AddDays(1);
        }

        return dias;
    }

    public bool SeTraslapaCon(RangoFechas otro)
    {
        return FechaInicio <= otro.FechaFin && FechaFin >= otro.FechaInicio;
    }

    public bool Equals(RangoFechas? other)
    {
        if (other is null) return false;
        return FechaInicio == other.FechaInicio && FechaFin == other.FechaFin;
    }

    public override bool Equals(object? obj) => Equals(obj as RangoFechas);

    public override int GetHashCode() => HashCode.Combine(FechaInicio, FechaFin);

    public static bool operator ==(RangoFechas? left, RangoFechas? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(RangoFechas? left, RangoFechas? right) => !(left == right);
}
