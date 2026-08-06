using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Saldos.Queries;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Web.Authorization;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

[Authorize]
public class SolicitudVacacionesController : Controller
{
    private readonly CrearSolicitudCommandHandler _crearHandler;
    private readonly EditarSolicitudCommandHandler _editarHandler;
    private readonly CancelarSolicitudCommandHandler _cancelarHandler;
    private readonly ObtenerMisSolicitudesQueryHandler _listarHandler;
    private readonly ObtenerSolicitudDetalleQueryHandler _detalleHandler;
    private readonly ObtenerSaldoQueryHandler _saldoHandler;
    private readonly IValidator<CrearSolicitudCommand> _crearValidator;
    private readonly IRepositorioEmpleado _repositorioEmpleados;
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;
    private readonly TimeProvider _timeProvider;

    public SolicitudVacacionesController(
        CrearSolicitudCommandHandler crearHandler,
        EditarSolicitudCommandHandler editarHandler,
        CancelarSolicitudCommandHandler cancelarHandler,
        ObtenerMisSolicitudesQueryHandler listarHandler,
        ObtenerSolicitudDetalleQueryHandler detalleHandler,
        ObtenerSaldoQueryHandler saldoHandler,
        IValidator<CrearSolicitudCommand> crearValidator,
        IRepositorioEmpleado repositorioEmpleados,
        IRepositorioSolicitudVacaciones repositorioSolicitudes,
        TimeProvider timeProvider)
    {
        _crearHandler = crearHandler;
        _editarHandler = editarHandler;
        _cancelarHandler = cancelarHandler;
        _listarHandler = listarHandler;
        _detalleHandler = detalleHandler;
        _saldoHandler = saldoHandler;
        _crearValidator = crearValidator;
        _repositorioEmpleados = repositorioEmpleados;
        _repositorioSolicitudes = repositorioSolicitudes;
        _timeProvider = timeProvider;
    }

    private Guid ObtenerEmpleadoId()
    {
        var claim = User.FindFirst("EmpleadoId")?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }

    private bool EsAprobador() => User.IsInRole(Roles.Aprobador);
    private bool EsRRHH() => User.IsInRole(Roles.RRHH);
    private bool EsSolicitudJson() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    private string ObtenerPrimerError()
    {
        var error = ModelState.Values
            .SelectMany(v => v.Errors)
            .FirstOrDefault();
        return error?.ErrorMessage ?? "Revise los datos ingresados.";
    }

    private async Task<EmpleadoDashboardViewModel> BuildDashboardAsync(Guid empleadoId, EstadoSolicitud? estado = null, int page = 1, int pageSize = 10)
    {
        var query = new ObtenerMisSolicitudesQuery(empleadoId, estado, page, pageSize);
        var resultado = await _listarHandler.HandleAsync(query);
        var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));
        var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(empleadoId);
        var todas = await _repositorioSolicitudes.ObtenerPorEmpleadoAsync(empleadoId);

        return new EmpleadoDashboardViewModel
        {
            Saldo = saldo,
            NombreEmpleado = empleado?.NombreCompleto ?? "Usuario",
            Solicitudes = resultado.Solicitudes,
            TotalCount = resultado.TotalCount,
            Page = resultado.Page,
            PageSize = resultado.PageSize,
            TotalPages = resultado.TotalPages,
            FiltroEstado = estado,
            Pendientes = todas.Count(s => s.Estado == EstadoSolicitud.Pending),
            Aprobadas = todas.Count(s => s.Estado == EstadoSolicitud.Approved),
            Rechazadas = todas.Count(s => s.Estado == EstadoSolicitud.Rejected),
            Canceladas = todas.Count(s => s.Estado == EstadoSolicitud.Cancelled),
            CrearSolicitud = new CrearSolicitudViewModel { SaldoDisponible = saldo?.SaldoDisponible ?? 0 }
        };
    }

    [HttpGet]
    public async Task<IActionResult> Index(EstadoSolicitud? estado, int page = 1, int pageSize = 10)
    {
        var empleadoId = ObtenerEmpleadoId();
        var viewModel = await BuildDashboardAsync(empleadoId, estado, page, pageSize);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var empleadoId = ObtenerEmpleadoId();
        var viewModel = await BuildDashboardAsync(empleadoId);
        viewModel.SheetAbierta = true;
        return View("Index", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearSolicitudViewModel formModel)
    {
        var empleadoId = ObtenerEmpleadoId();

        if (!ModelState.IsValid)
        {
            if (EsSolicitudJson())
            {
                return Json(new { success = false, error = ObtenerPrimerError() });
            }
            formModel.SaldoDisponible = (await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId)))?.SaldoDisponible ?? 0;
            var viewModel = await BuildDashboardAsync(empleadoId);
            viewModel.SheetAbierta = true;
            viewModel.CrearSolicitud = formModel;
            return View("Index", viewModel);
        }

        var command = new CrearSolicitudCommand(
            empleadoId,
            formModel.FechaInicio,
            formModel.FechaFin,
            formModel.Motivo ?? string.Empty);

        var validationResult = await _crearValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            if (EsSolicitudJson())
            {
                return Json(new { success = false, error = ObtenerPrimerError() });
            }
            formModel.SaldoDisponible = (await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId)))?.SaldoDisponible ?? 0;
            var viewModel = await BuildDashboardAsync(empleadoId);
            viewModel.SheetAbierta = true;
            viewModel.CrearSolicitud = formModel;
            return View("Index", viewModel);
        }

        try
        {
            var solicitudId = await _crearHandler.HandleAsync(command);

            // Si es AJAX, retornar JSON con el ID de la solicitud creada
            if (EsSolicitudJson())
            {
                return Json(new { success = true, solicitudId = solicitudId, mensaje = "Solicitud creada exitosamente." });
            }

            TempData["Mensaje"] = "Solicitud creada exitosamente.";
            return RedirectToAction(nameof(Detalle), new { id = solicitudId });
        }
        catch (SaldoInsuficienteException)
        {
            if (EsSolicitudJson())
            {
                return Json(new { success = false, error = "Saldo insuficiente para esta solicitud." });
            }
            ModelState.AddModelError(string.Empty, "Saldo insuficiente para esta solicitud.");
        }
        catch (TraslapeSolicitudesException)
        {
            if (EsSolicitudJson())
            {
                return Json(new { success = false, error = "La solicitud incluye días que ya están comprometidos en otra solicitud." });
            }
            ModelState.AddModelError(string.Empty, "La solicitud incluye días que ya están comprometidos en otra solicitud.");
        }
        catch (ArgumentException ex)
        {
            if (EsSolicitudJson())
            {
                return Json(new { success = false, error = ex.Message });
            }
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        formModel.SaldoDisponible = (await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId)))?.SaldoDisponible ?? 0;
        var viewModelError = await BuildDashboardAsync(empleadoId);
        viewModelError.SheetAbierta = true;
        viewModelError.CrearSolicitud = formModel;
        return View("Index", viewModelError);
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(Guid id)
    {
        var empleadoId = ObtenerEmpleadoId();

        try
        {
            var query = new ObtenerSolicitudDetalleQuery(id, empleadoId, EsAprobador(), EsRRHH());
            var solicitud = await _detalleHandler.HandleAsync(query);

            var fechaActual = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
            var esDueno = solicitud.EmpleadoId == empleadoId;

            var viewModel = new SolicitudDetalleViewModel
            {
                Id = solicitud.Id,
                NombreEmpleado = solicitud.EmpleadoNombre,
                FechaInicio = solicitud.FechaInicio,
                FechaFin = solicitud.FechaFin,
                DiasHabiles = solicitud.DiasRequeridos,
                Estado = solicitud.Estado,
                Comentario = solicitud.Motivo,
                FechaCreacion = solicitud.CreadoEn,
                Historial = solicitud.Historial,
                PuedeEditar = esDueno && solicitud.Estado == EstadoSolicitud.Pending,
                PuedeCancelar = esDueno && solicitud.Estado == EstadoSolicitud.Pending,
                EsAprobador = EsAprobador(),
                PuedeCancelarAprobada = EsAprobador() && solicitud.Estado == EstadoSolicitud.Approved && solicitud.FechaInicio > fechaActual
            };

            return View(viewModel);
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
        catch (AccesoNoAutorizadoException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var empleadoId = ObtenerEmpleadoId();

        try
        {
            var query = new ObtenerSolicitudDetalleQuery(id, empleadoId, false, false);
            var solicitud = await _detalleHandler.HandleAsync(query);

            if (solicitud.EmpleadoId != empleadoId)
            {
                return Forbid();
            }

            if (solicitud.Estado != EstadoSolicitud.Pending)
            {
                TempData["Error"] = "Solo se pueden editar solicitudes en estado Pending.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));

            var dashboard = await BuildDashboardAsync(empleadoId);
            dashboard.SheetAbierta = true;
            dashboard.EditarSolicitud = new EditarSolicitudViewModel
            {
                SolicitudId = solicitud.Id,
                FechaInicio = solicitud.FechaInicio,
                FechaFin = solicitud.FechaFin,
                Motivo = solicitud.Motivo,
                DiasActuales = solicitud.DiasRequeridos,
                SaldoDisponible = saldo?.SaldoDisponible ?? 0
            };

            return View("Index", dashboard);
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarSolicitudViewModel formModel)
    {
        var empleadoId = ObtenerEmpleadoId();

        if (!ModelState.IsValid)
        {
            var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));
            formModel.SaldoDisponible = saldo?.SaldoDisponible ?? 0;
            var dashboard = await BuildDashboardAsync(empleadoId);
            dashboard.SheetAbierta = true;
            dashboard.EditarSolicitud = formModel;
            return View("Index", dashboard);
        }

        var command = new EditarSolicitudCommand(
            formModel.SolicitudId,
            empleadoId,
            formModel.FechaInicio,
            formModel.FechaFin,
            formModel.Motivo ?? string.Empty);

        try
        {
            await _editarHandler.HandleAsync(command);
            TempData["Mensaje"] = "Solicitud editada exitosamente.";
            return RedirectToAction(nameof(Detalle), new { id = formModel.SolicitudId });
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
        catch (AccesoNoAutorizadoException)
        {
            return Forbid();
        }
        catch (SaldoInsuficienteException)
        {
            ModelState.AddModelError(string.Empty, "Saldo insuficiente para esta solicitud.");
        }
        catch (TraslapeSolicitudesException)
        {
            ModelState.AddModelError(string.Empty, "La solicitud incluye días que ya están comprometidos en otra solicitud.");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        var saldoRecarga = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));
        formModel.SaldoDisponible = saldoRecarga?.SaldoDisponible ?? 0;
        var dashboardError = await BuildDashboardAsync(empleadoId);
        dashboardError.SheetAbierta = true;
        dashboardError.EditarSolicitud = formModel;
        return View("Index", dashboardError);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var empleadoId = ObtenerEmpleadoId();

        try
        {
            var command = new CancelarSolicitudCommand(id, empleadoId);
            await _cancelarHandler.HandleAsync(command);
            TempData["Mensaje"] = "Solicitud cancelada exitosamente.";
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
        catch (AccesoNoAutorizadoException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> DetalleModal(Guid id)
    {
        var empleadoId = ObtenerEmpleadoId();

        try
        {
            var query = new ObtenerSolicitudDetalleQuery(id, empleadoId, EsAprobador(), EsRRHH());
            var solicitud = await _detalleHandler.HandleAsync(query);

            return PartialView("_DetalleModal", solicitud);
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
        catch (AccesoNoAutorizadoException)
        {
            return Forbid();
        }
    }
}
