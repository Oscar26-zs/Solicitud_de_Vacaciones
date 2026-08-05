using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Abstractions;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

/// <summary>CRUD de solicitudes del empleado autenticado (TASK-048).</summary>
[Authorize(Policy = "RequiereEmpleado")]
public sealed class SolicitudVacacionesController : Controller
{
    private readonly IUsuarioActual _usuario;
    private readonly CrearSolicitudCommandHandler _crearHandler;
    private readonly EditarSolicitudCommandHandler _editarHandler;
    private readonly CancelarSolicitudCommandHandler _cancelarHandler;
    private readonly ObtenerMisSolicitudesQueryHandler _misSolicitudes;
    private readonly ObtenerSolicitudDetalleQueryHandler _detalle;

    public SolicitudVacacionesController(
        IUsuarioActual usuario,
        CrearSolicitudCommandHandler crearHandler,
        EditarSolicitudCommandHandler editarHandler,
        CancelarSolicitudCommandHandler cancelarHandler,
        ObtenerMisSolicitudesQueryHandler misSolicitudes,
        ObtenerSolicitudDetalleQueryHandler detalle)
    {
        _usuario = usuario;
        _crearHandler = crearHandler;
        _editarHandler = editarHandler;
        _cancelarHandler = cancelarHandler;
        _misSolicitudes = misSolicitudes;
        _detalle = detalle;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? estado = null)
    {
        var empleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("Empleado no vinculado.");

        var resultado = await _misSolicitudes.HandleAsync(new ObtenerMisSolicitudesQuery
        {
            EmpleadoId = empleadoId,
            Estado = ParsearEstado(estado),
            Page = page,
            PageSize = pageSize,
        });

        return View(new ListaSolicitudesViewModel { PagedResult = resultado, EstadoFiltro = estado });
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(Guid id)
    {
        var empleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("Empleado no vinculado.");

        var resultado = await _detalle.HandleAsync(new ObtenerSolicitudDetalleQuery
        {
            SolicitudId = id,
            EmpleadoSolicitanteId = empleadoId,
        });

        return View(new DetalleSolicitudViewModel
        {
            Solicitud = resultado.Solicitud,
            Historial = resultado.Historial,
            EsDueno = resultado.Solicitud.EmpleadoId == empleadoId,
            EsAprobador = _usuario.Roles.Contains("Aprobador", StringComparer.OrdinalIgnoreCase),
        });
    }

    [HttpGet]
    public IActionResult Crear()
        => View(new CrearSolicitudViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearSolicitudViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var empleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("Empleado no vinculado.");

        try
        {
            await _crearHandler.HandleAsync(new CrearSolicitudCommand
            {
                EmpleadoId = empleadoId,
                FechaInicio = model.FechaInicio,
                FechaFin = model.FechaFin,
                Motivo = model.Motivo,
            });

            TempData["Exito"] = "Solicitud creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex));
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(Guid id)
    {
        var empleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("Empleado no vinculado.");

        var resultado = await _detalle.HandleAsync(new ObtenerSolicitudDetalleQuery
        {
            SolicitudId = id,
            EmpleadoSolicitanteId = empleadoId,
        });

        if (resultado.Solicitud.Estado != nameof(EstadoSolicitud.Pending))
        {
            return RedirectToAction(nameof(Detalle), new { id });
        }

        return View(new EditarSolicitudViewModel
        {
            SolicitudId = resultado.Solicitud.Id,
            FechaInicio = resultado.Solicitud.FechaInicio,
            FechaFin = resultado.Solicitud.FechaFin,
            Motivo = resultado.Solicitud.Motivo,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, EditarSolicitudViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var empleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("Empleado no vinculado.");

        model.SolicitudId = id;

        try
        {
            await _editarHandler.HandleAsync(new EditarSolicitudCommand
            {
                SolicitudId = id,
                EmpleadoId = empleadoId,
                FechaInicio = model.FechaInicio,
                FechaFin = model.FechaFin,
                Motivo = model.Motivo,
            });

            TempData["Exito"] = "Solicitud actualizada.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, MensajeError(ex));
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var empleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("Empleado no vinculado.");

        try
        {
            await _cancelarHandler.HandleAsync(new CancelarSolicitudCommand
            {
                SolicitudId = id,
                EmpleadoId = empleadoId,
            });

            TempData["Exito"] = "Solicitud cancelada.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = MensajeError(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    private static EstadoSolicitud? ParsearEstado(string? estado)
        => Enum.TryParse<EstadoSolicitud>(estado, ignoreCase: true, out var e) ? e : null;

    private static string MensajeError(Exception ex)
    {
        return ex switch
        {
            Domain.Exceptions.SaldoInsuficienteException => "Saldo insuficiente para realizar la operación.",
            Domain.Exceptions.TraslapeSolicitudesException => "La solicitud se traslapa con otra solicitud existente.",
            Domain.Exceptions.SolicitudNoEncontradaException => "La solicitud no existe.",
            Domain.Exceptions.TransicionEstadoInvalidaException => "La operación no es válida para el estado actual de la solicitud.",
            Domain.Exceptions.AutoAprobacionNoPermitidaException => "No se puede aprobar la propia solicitud.",
            Domain.Exceptions.AprobadorInactivoException => "El aprobador está inactivo.",
            Domain.Exceptions.AccesoNoPermitidoException => "No tiene permiso para realizar esta acción.",
            Domain.Exceptions.ConcurrenciaException => "El registro fue modificado por otro usuario. Reintente la operación.",
            FluentValidation.ValidationException ve => ve.Errors.FirstOrDefault()?.ErrorMessage ?? "Los datos proporcionados no son válidos.",
            _ => "Ocurrió un error al procesar la solicitud.",
        };
    }
}