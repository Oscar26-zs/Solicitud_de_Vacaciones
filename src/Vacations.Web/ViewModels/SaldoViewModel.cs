using Vacations.Application.Saldos.Queries;

namespace Vacations.Web.ViewModels;

public class SaldoViewModel
{
    public SaldoDto? Saldo { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
}
