using Vacations.Domain.Exceptions;

namespace Vacations.Domain.ValueObjects;

/// <summary>
/// Value object inmutable que encapsula la fecha de inicio y fin de una
/// solicitud de vacaciones. Garantiza los invariantes de fechas:
/// inicio ≥ mañana, fin ≥ inicio y horizonte máximo de 2 meses (RN-05, RN-06, RN-31).
/// </summary>
public sealed class RangoFechas : IEquatable<RangoFechas>
{
    private const int HorizonteMesesMaximo = 2;

    public DateTime FechaInicio { get; }

    public DateTime FechaFin { get; }

    private RangoFechas(DateTime fechaInicio, DateTime fechaFin)
    {
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }

    /// <summary>
    /// Crea un rango de fechas validando las reglas de negocio de fechas.
    /// </summary>
    /// <param name="fechaInicio">Fecha de inicio (fecha pura, sin hora).</param>
    /// <param name="fechaFin">Fecha de fin (fecha pura, sin hora).</param>
    /// <param name="fechaActual">Fecha actual provista por el sistema (TimeProvider).</param>
    /// <exception cref="ArgumentException">Si las fechas no cumplen las invariantes.</exception>
    public static RangoFechas Crear(DateTime fechaInicio, DateTime fechaFin, DateTime fechaActual)
    {
        var hoy = fechaActual.Date;
        var inicio = fechaInicio.Date;
        var fin = fechaFin.Date;

        var manana = hoy.AddDays(1);
        if (inicio < manana)
        {
            throw new ArgumentException("La fecha de inicio no puede ser anterior a mañana", nameof(fechaInicio));
        }

        if (fin < inicio)
        {
            throw new ArgumentException("La fecha de fin no puede ser anterior a la de inicio", nameof(fechaFin));
        }

        if (inicio > hoy.AddMonths(HorizonteMesesMaximo))
        {
            throw new ArgumentException($"La fecha de inicio no puede exceder los {HorizonteMesesMaximo} meses desde la fecha actual", nameof(fechaInicio));
        }

        return new RangoFechas(inicio, fin);
    }

    /// <summary>
    /// Calcula los días hábiles del rango (inclusivo), excluyendo sábados y
    /// domingos. Los feriados NO se excluyen y cuentan para el consumo de saldo
    /// (RN-25, RF-002).
    /// </summary>
    public int CalcularDiasHabiles()
    {
        var dias = 0;
        for (var dia = FechaInicio; dia <= FechaFin; dia = dia.AddDays(1))
        {
            if (dia.DayOfWeek != DayOfWeek.Saturday && dia.DayOfWeek != DayOfWeek.Sunday)
            {
                dias++;
            }
        }

        return dias;
    }

    public bool SeSolapaCon(RangoFechas otro)
        => FechaInicio <= otro.FechaFin && otro.FechaInicio <= FechaFin;

    public bool Equals(RangoFechas? other)
        => other is not null && FechaInicio == other.FechaInicio && FechaFin == other.FechaFin;

    public override bool Equals(object? obj) => Equals(obj as RangoFechas);

    public override int GetHashCode() => HashCode.Combine(FechaInicio, FechaFin);

    public static bool operator ==(RangoFechas? left, RangoFechas? right) => Equals(left, right);

    public static bool operator !=(RangoFechas? left, RangoFechas? right) => !Equals(left, right);
}