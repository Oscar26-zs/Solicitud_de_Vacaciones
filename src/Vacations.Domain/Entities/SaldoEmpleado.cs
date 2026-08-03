using Vacations.Domain.Exceptions;

namespace Vacations.Domain.Entities;

/// <summary>
/// Gestiona los días de vacaciones de un empleado.
/// Formula: <c>SaldoDisponible = SaldoAcumulado - SaldoConsumido - SaldoPendiente</c>.
/// Garantiza el invariante de saldo no negativo (RN-01, RN-02, RN-03, RN-04, RN-24).
/// </summary>
public sealed class SaldoEmpleado
{
    public Guid Id { get; private set; }

    public Guid EmpleadoId { get; private set; }

    /// <summary>Días acumulados (1 por mes completo laborado) con carry-over ilimitado.</summary>
    public int SaldoAcumulado { get; private set; }

    /// <summary>Días consumidos por solicitudes aprobadas.</summary>
    public int SaldoConsumido { get; private set; }

    /// <summary>Días comprometidos por solicitudes Pending (congelados).</summary>
    public int SaldoPendiente { get; private set; }

    public DateTime UltimaActualizacion { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    /// <summary>Propiedad calculada: acumulado − consumido − pendiente. No se persiste.</summary>
    public int SaldoDisponible => SaldoAcumulado - SaldoConsumido - SaldoPendiente;

    private SaldoEmpleado(Guid id, Guid empleadoId, DateTime ultimaActualizacion)
    {
        Id = id;
        EmpleadoId = empleadoId;
        UltimaActualizacion = ultimaActualizacion;
    }

    public static SaldoEmpleado Crear(Guid empleadoId, DateTime fechaActual)
    {
        return new SaldoEmpleado(Guid.NewGuid(), empleadoId, fechaActual.Date);
    }

    /// <summary>CU-01: acumula días por meses completos laborados.</summary>
    public void AcumularDias(int dias, DateTime fechaActual)
    {
        if (dias <= 0)
        {
            throw new ArgumentException("La cantidad de días a acumular debe ser mayor a cero", nameof(dias));
        }

        SaldoAcumulado += dias;
        UltimaActualizacion = fechaActual.Date;
    }

    /// <summary>Crea una solicitud: congela días en pendiente.</summary>
    public void CongelarSaldo(int dias, DateTime fechaActual)
    {
        ValidarSuficiente(dias);
        SaldoPendiente += dias;
        UltimaActualizacion = fechaActual.Date;
    }

    /// <summary>Aprueba una solicitud: mueve días de pendiente a consumido.</summary>
    public void DescontarSaldo(int dias, DateTime fechaActual)
    {
        if (dias <= 0)
        {
            throw new ArgumentException("La cantidad de días a descontar debe ser mayor a cero", nameof(dias));
        }

        if (dias > SaldoPendiente)
        {
            throw new SaldoInsuficienteException("No se puede aprobar: saldo insuficiente al momento de la aprobación");
        }

        SaldoPendiente -= dias;
        SaldoConsumido += dias;
        UltimaActualizacion = fechaActual.Date;
    }

    /// <summary>Rechaza/cancela/expira: libera días congelados en pendiente.</summary>
    public void LiberarSaldoPendiente(int dias, DateTime fechaActual)
    {
        if (dias <= 0)
        {
            throw new ArgumentException("La cantidad de dias a liberar debe ser mayor a cero", nameof(dias));
        }

        if (dias > SaldoPendiente)
        {
            dias = SaldoPendiente;
        }

        SaldoPendiente -= dias;
        UltimaActualizacion = fechaActual.Date;
    }

    /// <summary>Cancela una solicitud aprobada: devuelve días consumidos al disponible.</summary>
    public void RestaurarSaldo(int dias, DateTime fechaActual)
    {
        if (dias <= 0)
        {
            throw new ArgumentException("La cantidad de dias a restaurar debe ser mayor a cero", nameof(dias));
        }

        if (dias > SaldoConsumido)
        {
            dias = SaldoConsumido;
        }

        SaldoConsumido -= dias;
        UltimaActualizacion = fechaActual.Date;
    }

    private void ValidarSuficiente(int dias)
    {
        if (SaldoDisponible < dias)
        {
            throw new SaldoInsuficienteException();
        }
    }
}