using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Saldos.Queries;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;
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
    private readonly TimeProvider _timeProvider;

    public SolicitudVacacionesController(
        CrearSolicitudCommandHandler crearHandler,
        EditarSolicitudCommandHandler editarHandler,
        CancelarSolicitudCommandHandler cancelarHandler,
        ObtenerMisSolicitudesQueryHandler listarHandler,
        ObtenerSolicitudDetalleQueryHandler detalleHandler,
        ObtenerSaldoQueryHandler saldoHandler,
        IValidator<CrearSolicitudCommand> crearValidator,
        TimeProvider timeProvider)
    {
        _crearHandler = crearHandler;
        _editarHandler = editarHandler;
        _cancelarHandler = cancelarHandler;
        _listarHandler = listarHandler;
        _detalleHandler = detalleHandler;
        _saldoHandler = saldoHandler;
        _crearValidator = crearValidator;
        _timeProvider = timeProvider;
    }

    private Guid ObtenerEmpleadoId()
    {
        var claim = User.FindFirst("EmpleadoId")?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }

    private bool EsAprobador() => User.IsInRole(Roles.Aprobador);
    private bool EsRRHH() => User.IsInRole(Roles.RRHH);

    [HttpGet]
    public async Task<IActionResult> Index(EstadoSolicitud? estado, int page = 1, int pageSize = 10)
    {
        var empleadoId = ObtenerEmpleadoId();
        var query = new ObtenerMisSolicitudesQuery(empleadoId, estado, page, pageSize);
        var resultado = await _listarHandler.HandleAsync(query);

        var viewModel = new SolicitudListaViewModel
        {
            Solicitudes = resultado.Solicitudes,
            TotalCount = resultado.TotalCount,
            Page = resultado.Page,
            PageSize = resultado.PageSize,
            TotalPages = resultado.TotalPages,
            FiltroEstado = estado
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var empleadoId = ObtenerEmpleadoId();
        var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));

        var viewModel = new CrearSolicitudViewModel
        {
            SaldoDisponible = saldo?.SaldoDisponible ?? 0
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearSolicitudViewModel viewModel)
    {
        var empleadoId = ObtenerEmpleadoId();

        if (!ModelState.IsValid)
        {
            var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));
            viewModel.SaldoDisponible = saldo?.SaldoDisponible ?? 0;
            return View(viewModel);
        }

        var command = new CrearSolicitudCommand(
            empleadoId,
            viewModel.FechaInicio,
            viewModel.FechaFin,
            viewModel.Comentario ?? string.Empty);

        var validationResult = await _crearValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));
            viewModel.SaldoDisponible = saldo?.SaldoDisponible ?? 0;
            return View(viewModel);
        }

        try
        {
            var solicitudId = await _crearHandler.HandleAsync(command);
            TempData["Mensaje"] = "Solicitud creada exitosamente.";
            return RedirectToAction(nameof(Detalle), new { id = solicitudId });
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

        var saldoRecarga = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));
        viewModel.SaldoDisponible = saldoRecarga?.SaldoDisponible ?? 0;
        return View(viewModel);
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

            var viewModel = new EditarSolicitudViewModel
            {
                SolicitudId = solicitud.Id,
                FechaInicio = solicitud.FechaInicio,
                FechaFin = solicitud.FechaFin,
                Comentario = solicitud.Motivo,
                DiasActuales = solicitud.DiasRequeridos,
                SaldoDisponible = saldo?.SaldoDisponible ?? 0
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
    public async Task<IActionResult> Editar(EditarSolicitudViewModel viewModel)
    {
        var empleadoId = ObtenerEmpleadoId();

        if (!ModelState.IsValid)
        {
            var saldo = await _saldoHandler.HandleAsync(new ObtenerSaldoQuery(empleadoId));
            viewModel.SaldoDisponible = saldo?.SaldoDisponible ?? 0;
            return View(viewModel);
        }

        var command = new EditarSolicitudCommand(
            viewModel.SolicitudId,
            empleadoId,
            viewModel.FechaInicio,
            viewModel.FechaFin,
            viewModel.Comentario ?? string.Empty);

        try
        {
            await _editarHandler.HandleAsync(command);
            TempData["Mensaje"] = "Solicitud editada exitosamente.";
            return RedirectToAction(nameof(Detalle), new { id = viewModel.SolicitudId });
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
        viewModel.SaldoDisponible = saldoRecarga?.SaldoDisponible ?? 0;
        return View(viewModel);
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
}
