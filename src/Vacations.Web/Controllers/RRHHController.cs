using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Abstractions;
using Vacations.Application.Saldos.Queries;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

[Authorize(Policy = "RequiereRRHH")]
public sealed class RRHHController : Controller
{
    private readonly ObtenerHistorialRRHHQueryHandler _historialHandler;
    private readonly ObtenerSaldoQueryHandler _saldoHandler;
    private readonly IUsuarioActual _usuario;

    public RRHHController(
        ObtenerHistorialRRHHQueryHandler historialHandler,
        ObtenerSaldoQueryHandler saldoHandler,
        IUsuarioActual usuario)
    {
        _historialHandler = historialHandler;
        _saldoHandler = saldoHandler;
        _usuario = usuario;
    }

    [HttpGet]
    public async Task<IActionResult> Solicitudes(int page = 1, int pageSize = 10)
    {
        var resultado = await _historialHandler.HandleAsync(new ObtenerHistorialRRHHQuery
        {
            Page = page,
            PageSize = pageSize,
        });

        return View(new ConsultaRRHHViewModel
        {
            PagedResult = resultado,
            Filtros = new FiltrosRRHHViewModel(),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Solicitudes(FiltrosRRHHViewModel filtros, int page = 1, int pageSize = 10)
    {
        var resultado = await _historialHandler.HandleAsync(new ObtenerHistorialRRHHQuery
        {
            Estado = Enum.TryParse<EstadoSolicitud>(filtros.Estado, ignoreCase: true, out var e) ? e : null,
            EmpleadoId = filtros.EmpleadoId,
            FechaInicio = filtros.FechaInicio,
            FechaFin = filtros.FechaFin,
            Page = page,
            PageSize = pageSize,
        });

        return View(new ConsultaRRHHViewModel
        {
            PagedResult = resultado,
            Filtros = filtros,
        });
    }

    [HttpGet]
    public async Task<IActionResult> SaldoEmpleado(Guid empleadoId)
    {
        var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery
        {
            EmpleadoId = empleadoId,
            EmpleadoSolicitanteId = null,
            EsRRHH = true,
        });

        return View(new SaldoEmpleadoRRHHViewModel
        {
            EmpleadoId = empleadoId,
            EmpleadoNombre = string.Empty,
            Saldo = saldo,
        });
    }
}