using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Abstractions;
using Vacations.Application.Saldos.Queries;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

/// <summary>Consulta de saldo del empleado autenticado (TASK-049, HU-04).</summary>
[Authorize]
public sealed class SaldoController : Controller
{
    private readonly ObtenerSaldoQueryHandler _saldoHandler;
    private readonly IUsuarioActual _usuario;

    public SaldoController(ObtenerSaldoQueryHandler saldoHandler, IUsuarioActual usuario)
    {
        _saldoHandler = saldoHandler;
        _usuario = usuario;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var empleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("Empleado no vinculado.");

        var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery
        {
            EmpleadoId = empleadoId,
            EmpleadoSolicitanteId = empleadoId,
        });

        return View(new SaldoViewModel { Saldo = saldo });
    }
}