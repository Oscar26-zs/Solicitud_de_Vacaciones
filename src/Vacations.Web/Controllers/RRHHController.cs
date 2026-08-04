using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Saldos.Queries;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Enums;
using Vacations.Web.Authorization;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

[Authorize(Roles = Roles.RRHH)]
public class RRHHController : Controller
{
    private readonly ObtenerSolicitudesRRHHQueryHandler _solicitudesHandler;
    private readonly ObtenerSaldoQueryHandler _saldoHandler;
    private readonly IRepositorioEmpleado _repositorioEmpleados;

    public RRHHController(
        ObtenerSolicitudesRRHHQueryHandler solicitudesHandler,
        ObtenerSaldoQueryHandler saldoHandler,
        IRepositorioEmpleado repositorioEmpleados)
    {
        _solicitudesHandler = solicitudesHandler;
        _saldoHandler = saldoHandler;
        _repositorioEmpleados = repositorioEmpleados;
    }

    [HttpGet]
    public async Task<IActionResult> Solicitudes(
        Guid? empleadoId,
        EstadoSolicitud? estado,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        int page = 1,
        int pageSize = 10)
    {
        var query = new ObtenerSolicitudesRRHHQuery(empleadoId, estado, fechaDesde, fechaHasta, page, pageSize);
        var resultado = await _solicitudesHandler.HandleAsync(query);

        var viewModel = new ConsultaRRHHViewModel
        {
            Solicitudes = resultado.Solicitudes,
            TotalCount = resultado.TotalCount,
            Page = resultado.Page,
            PageSize = resultado.PageSize,
            TotalPages = resultado.TotalPages,
            FiltroEmpleadoId = empleadoId,
            FiltroEstado = estado,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SaldoEmpleado(Guid empleadoId)
    {
        var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));
        var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(empleadoId);

        if (empleado == null)
        {
            return NotFound();
        }

        var viewModel = new SaldoViewModel
        {
            Saldo = saldo,
            NombreEmpleado = empleado.NombreCompleto
        };

        return View(viewModel);
    }
}
