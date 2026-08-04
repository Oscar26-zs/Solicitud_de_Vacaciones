using Vacations.Domain.Exceptions;

namespace Vacations.Domain.Entities;

public sealed class SaldoEmpleado
{
    public Guid Id { get; private set; }
    public Guid EmpleadoId { get; private set; }
    public int SaldoAcumulado { get; private set; }
    public int SaldoConsumido { get; private set; }
    public int SaldoPendiente { get; private set; }
    public DateTime UltimaActualizacion { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public int SaldoDisponible => SaldoAcumulado - SaldoConsumido - SaldoPendiente;

    private SaldoEmpleado()
    {
    }

    public static SaldoEmpleado Crear(Guid empleadoId, DateTime fechaActual)
    {
        return new SaldoEmpleado
        {
            Id = Guid.NewGuid(),
            EmpleadoId = empleadoId,
            SaldoAcumulado = 0,
            SaldoConsumido = 0,
            SaldoPendiente = 0,
            UltimaActualizacion = fechaActual
        };
    }

    public void AcumularDias(int dias, DateTime fechaActual)
    {
        if (dias <= 0)
        {
            throw new ArgumentException("Los días a acumular deben ser mayores a cero.", nameof(dias));
        }

        SaldoAcumulado += dias;
        UltimaActualizacion = fechaActual;
    }

    public void CongelarSaldo(int dias, DateTime fechaActual)
    {
        if (dias <= 0)
        {
            throw new ArgumentException("Los días a congelar deben ser mayores a cero.", nameof(dias));
        }

        if (dias > SaldoDisponible)
        {
            throw new SaldoInsuficienteException(SaldoDisponible, dias);
        }

        SaldoPendiente += dias;
        UltimaActualizacion = fechaActual;
    }

    public void DescontarSaldo(int dias, DateTime fechaActual)
    {
        if (dias <= 0)
        {
            throw new ArgumentException("Los días a descontar deben ser mayores a cero.", nameof(dias));
        }

        if (dias > SaldoPendiente)
        {
            throw new InvalidOperationException("No se puede descontar más días de los que están pendientes.");
        }

        SaldoPendiente -= dias;
        SaldoConsumido += dias;
        UltimaActualizacion = fechaActual;
    }

    public void LiberarSaldoPendiente(int dias, DateTime fechaActual)
    {
        if (dias <= 0)
        {
            throw new ArgumentException("Los días a liberar deben ser mayores a cero.", nameof(dias));
        }

        if (dias > SaldoPendiente)
        {
            throw new InvalidOperationException("No se puede liberar más días de los que están pendientes.");
        }

        SaldoPendiente -= dias;
        UltimaActualizacion = fechaActual;
    }

    public void RestaurarSaldo(int dias, DateTime fechaActual)
    {
        if (dias <= 0)
        {
            throw new ArgumentException("Los días a restaurar deben ser mayores a cero.", nameof(dias));
        }

        if (dias > SaldoConsumido)
        {
            throw new InvalidOperationException("No se puede restaurar más días de los consumidos.");
        }

        SaldoConsumido -= dias;
        UltimaActualizacion = fechaActual;
    }

    public void AjustarSaldoPendiente(int diasAnteriores, int diasNuevos, DateTime fechaActual)
    {
        var diferencia = diasNuevos - diasAnteriores;

        if (diferencia > 0)
        {
            if (diferencia > SaldoDisponible)
            {
                throw new SaldoInsuficienteException(SaldoDisponible, diferencia);
            }
            SaldoPendiente += diferencia;
        }
        else if (diferencia < 0)
        {
            SaldoPendiente += diferencia;
        }

        UltimaActualizacion = fechaActual;
    }
}
