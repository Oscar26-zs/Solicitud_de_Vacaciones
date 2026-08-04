using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Saldos.Queries;
using Vacations.Domain.Abstractions;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

[Authorize]
public class SaldoController : Controller
{
    private readonly ObtenerSaldoQueryHandler _saldoHandler;
    private readonly IRepositorioEmpleado _repositorioEmpleados;

    public SaldoController(
        ObtenerSaldoQueryHandler saldoHandler,
        IRepositorioEmpleado repositorioEmpleados)
    {
        _saldoHandler = saldoHandler;
        _repositorioEmpleados = repositorioEmpleados;
    }

    private Guid ObtenerEmpleadoId()
    {
        var claim = User.FindFirst("EmpleadoId")?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var empleadoId = ObtenerEmpleadoId();
        var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));
        var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(empleadoId);

        var viewModel = new SaldoViewModel
        {
            Saldo = saldo,
            NombreEmpleado = empleado?.NombreCompleto ?? "Usuario"
        };

        return View(viewModel);
    }
}
