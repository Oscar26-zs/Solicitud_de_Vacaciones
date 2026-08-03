using Vacations.Application.Common;

namespace Vacations.Web.ViewModels;

/// <summary>ViewModel de saldo de un empleado consultado por RRHH (CU-02, solo lectura).</summary>
public sealed class SaldoEmpleadoRRHHViewModel
{
    public Guid EmpleadoId { get; set; }

    public string EmpleadoNombre { get; set; } = default!;

    public SaldoDto Saldo { get; set; } = default!;
}