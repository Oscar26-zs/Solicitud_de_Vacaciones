using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Web.Authorization;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

[Authorize(Roles = Roles.Aprobador)]
public class BandejaAprobadorController : Controller
{
    private readonly ObtenerBandejaAprobadorQueryHandler _bandejaHandler;
    private readonly ObtenerDetalleAprobacionQueryHandler _detalleAprobacionHandler;
    private readonly ObtenerSolicitudDetalleQueryHandler _detalleHandler;
    private readonly AprobarSolicitudCommandHandler _aprobarHandler;
    private readonly RechazarSolicitudCommandHandler _rechazarHandler;
    private readonly CancelarAprobadaCommandHandler _cancelarAprobadaHandler;
    private readonly IValidator<RechazarSolicitudCommand> _rechazarValidator;
    private readonly TimeProvider _timeProvider;

    public BandejaAprobadorController(
        ObtenerBandejaAprobadorQueryHandler bandejaHandler,
        ObtenerDetalleAprobacionQueryHandler detalleAprobacionHandler,
        ObtenerSolicitudDetalleQueryHandler detalleHandler,
        AprobarSolicitudCommandHandler aprobarHandler,
        RechazarSolicitudCommandHandler rechazarHandler,
        CancelarAprobadaCommandHandler cancelarAprobadaHandler,
        IValidator<RechazarSolicitudCommand> rechazarValidator,
        TimeProvider timeProvider)
    {
        _bandejaHandler = bandejaHandler;
        _detalleAprobacionHandler = detalleAprobacionHandler;
        _detalleHandler = detalleHandler;
        _aprobarHandler = aprobarHandler;
        _rechazarHandler = rechazarHandler;
        _cancelarAprobadaHandler = cancelarAprobadaHandler;
        _rechazarValidator = rechazarValidator;
        _timeProvider = timeProvider;
    }

    private Guid ObtenerEmpleadoId()
    {
        var claim = User.FindFirst("EmpleadoId")?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }

    private bool EsSolicitudJson() =>
        Request.Headers.Accept.ToString().Contains("application/json") ||
        Request.Headers.XRequestedWith == "XMLHttpRequest";

    [HttpGet]
    public async Task<IActionResult> Index(
        string? filtroEmpleado,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        EstadoSolicitud? estado,
        int page = 1,
        int pageSize = 10)
    {
        var aprobadorId = ObtenerEmpleadoId();
        var query = new ObtenerBandejaAprobadorQuery(aprobadorId, filtroEmpleado, fechaDesde, fechaHasta, estado, page, pageSize);
        var resultado = await _bandejaHandler.HandleAsync(query);

        var viewModel = new BandejaAprobadorViewModel
        {
            Solicitudes = resultado.Solicitudes,
            TotalCount = resultado.TotalCount,
            Page = resultado.Page,
            PageSize = resultado.PageSize,
            TotalPages = resultado.TotalPages,
            FiltroEmpleado = filtroEmpleado,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            FiltroEstado = estado,
            Pendientes = resultado.Estadisticas.Pendientes,
            Aprobadas = resultado.Estadisticas.Aprobadas,
            Rechazadas = resultado.Estadisticas.Rechazadas,
            Colaboradores = resultado.Estadisticas.Colaboradores,
            DiasAprobados = resultado.Estadisticas.DiasAprobados
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> DetalleModal(Guid id)
    {
        var aprobadorId = ObtenerEmpleadoId();

        try
        {
            var dto = await _detalleAprobacionHandler.HandleAsync(
                new ObtenerDetalleAprobacionQuery(id, aprobadorId));
            return PartialView("_DetalleAprobacionModal", DetalleAprobacionViewModel.FromDto(dto));
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(Guid id)
    {
        var aprobadorId = ObtenerEmpleadoId();

        try
        {
            var query = new ObtenerSolicitudDetalleQuery(id, aprobadorId, true, false);
            var solicitud = await _detalleHandler.HandleAsync(query);

            var fechaActual = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);

            var viewModel = new DetalleSolicitudViewModel
            {
                Solicitud = solicitud,
                PuedeEditar = false,
                PuedeCancelar = false,
                EsAprobador = true,
                PuedeCancelarAprobada = solicitud.Estado == EstadoSolicitud.Approved && solicitud.FechaInicio > fechaActual
            };

            return View(viewModel);
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(Guid id)
    {
        var aprobadorId = ObtenerEmpleadoId();
        var command = new AprobarSolicitudCommand(id, aprobadorId);

        try
        {
            await _aprobarHandler.HandleAsync(command);

            if (EsSolicitudJson())
            {
                return Json(new { ok = true, message = $"Solicitud {Folio(id)} aprobada." });
            }

            TempData["Mensaje"] = "Solicitud aprobada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
        catch (AutoAprobacionNoPermitidaException)
        {
            if (EsSolicitudJson()) return Json(new { ok = false, message = "No puede aprobar sus propias solicitudes." });
            TempData["Error"] = "No puede aprobar sus propias solicitudes.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        catch (AprobadorInactivoException)
        {
            if (EsSolicitudJson()) return Json(new { ok = false, message = "Su cuenta de aprobador no está activa." });
            TempData["Error"] = "Su cuenta de aprobador no está activa.";
            return RedirectToAction(nameof(Index));
        }
        catch (TransicionEstadoInvalidaException)
        {
            if (EsSolicitudJson()) return Json(new { ok = false, message = "La solicitud ya no está en estado Pendiente." });
            TempData["Error"] = "La solicitud ya no está en estado Pending.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Rechazar(Guid id)
    {
        var aprobadorId = ObtenerEmpleadoId();

        try
        {
            var query = new ObtenerSolicitudDetalleQuery(id, aprobadorId, true, false);
            var solicitud = await _detalleHandler.HandleAsync(query);

            if (solicitud.Estado != EstadoSolicitud.Pending)
            {
                TempData["Error"] = "Solo se pueden rechazar solicitudes en estado Pending.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new RechazarSolicitudViewModel
            {
                SolicitudId = solicitud.Id,
                EmpleadoNombre = solicitud.EmpleadoNombre,
                FechaInicio = solicitud.FechaInicio,
                FechaFin = solicitud.FechaFin,
                DiasRequeridos = solicitud.DiasRequeridos
            };

            return View(viewModel);
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(RechazarSolicitudViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            if (EsSolicitudJson())
            {
                return Json(new { ok = false, message = "El comentario es obligatorio al rechazar una solicitud." });
            }
            return View(viewModel);
        }

        var aprobadorId = ObtenerEmpleadoId();
        var command = new RechazarSolicitudCommand(viewModel.SolicitudId, aprobadorId, viewModel.Comentario);

        var validationResult = await _rechazarValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            if (EsSolicitudJson())
            {
                return Json(new { ok = false, message = validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Datos inválidos." });
            }
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(viewModel);
        }

        try
        {
            await _rechazarHandler.HandleAsync(command);

            if (EsSolicitudJson())
            {
                return Json(new { ok = true, message = $"Solicitud {Folio(viewModel.SolicitudId)} rechazada." });
            }

            TempData["Mensaje"] = "Solicitud rechazada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
        catch (AutoAprobacionNoPermitidaException)
        {
            if (EsSolicitudJson()) return Json(new { ok = false, message = "No puede rechazar sus propias solicitudes." });
            TempData["Error"] = "No puede rechazar sus propias solicitudes.";
            return RedirectToAction(nameof(Index));
        }
        catch (AprobadorInactivoException)
        {
            if (EsSolicitudJson()) return Json(new { ok = false, message = "Su cuenta de aprobador no está activa." });
            TempData["Error"] = "Su cuenta de aprobador no está activa.";
            return RedirectToAction(nameof(Index));
        }
        catch (TransicionEstadoInvalidaException)
        {
            if (EsSolicitudJson()) return Json(new { ok = false, message = "La solicitud ya no está en estado Pendiente." });
            TempData["Error"] = "La solicitud ya no está en estado Pending.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarAprobada(Guid id)
    {
        var aprobadorId = ObtenerEmpleadoId();
        var command = new CancelarAprobadaCommand(id, aprobadorId);

        try
        {
            await _cancelarAprobadaHandler.HandleAsync(command);
            TempData["Mensaje"] = "Solicitud cancelada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound();
        }
        catch (CancelacionNoPermitidaException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Detalle), new { id });
        }
        catch (TransicionEstadoInvalidaException)
        {
            TempData["Error"] = "Solo se pueden cancelar solicitudes aprobadas.";
            return RedirectToAction(nameof(Index));
        }
    }

    private static string Folio(Guid id) => "SOL-" + id.ToString("N").Substring(0, 6).ToUpper();
}
