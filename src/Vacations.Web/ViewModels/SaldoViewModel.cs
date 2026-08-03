using Vacations.Application.Common;

namespace Vacations.Web.ViewModels;

/// <summary>ViewModel de saldo del empleado con tarjetas de estadística (HU-04).</summary>
public sealed class SaldoViewModel
{
    public SaldoDto Saldo { get; set; } = default!;

    public double PorcentajeUsado
        => Saldo.Acumulado > 0 ? (double)Saldo.Consumido / Saldo.Acumulado * 100 : 0;
}