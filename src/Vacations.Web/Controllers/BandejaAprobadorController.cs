using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Abstractions;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Web.ViewModels;

namespace Vacations.Web.Controllers;

[Authorize(Policy = "RequiereAprobador")]
public sealed class BandejaAprobadorController : Controller
{
    private readonly IUsuarioActual _usuario;
    private readonly ObtenerBandejaAprobadorQueryHandler _bandeja;
    private readonly ObtenerSolicitudDetalleQueryHandler _detalleHandler;
    private readonly AprobarSolicitudCommandHandler _aprobarHandler;
    private readonly RechazarSolicitudCommandHandler _rechazarHandler;
    private readonly CancelarAprobadaCommandHandler _cancelarAprobadaHandler;

    public BandejaAprobadorController(
        IUsuarioActual usuario,
        ObtenerBandejaAprobadorQueryHandler bandeja,
        ObtenerSolicitudDetalleQueryHandler detalleHandler,
        AprobarSolicitudCommandHandler aprobarHandler,
        RechazarSolicitudCommandHandler rechazarHandler,
        CancelarAprobadaCommandHandler cancelarAprobadaHandler)
    {
        _usuario = usuario;
        _bandeja = bandeja;
        _detalleHandler = detalleHandler;
        _aprobarHandler = aprobarHandler;
        _rechazarHandler = rechazarHandler;
        _cancelarAprobadaHandler = cancelarAprobadaHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var aprobadorEmpleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("El empleado aprobador no está vinculado.");

        var query = new ObtenerBandejaAprobadorQuery
        {
            AprobadorEmpleadoId = aprobadorEmpleadoId,
            Page = page,
            PageSize = pageSize,
        };

        var resultado = await _bandeja.HandleAsync(query);

        return View(new BandejaAprobadorViewModel { PagedResult = resultado });
    }

    [HttpGet]
    public async Task<IActionResult> Detalle(Guid id)
    {
        var empleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("El empleado aprobador no está vinculado.");

        var resultado = await _detalleHandler.HandleAsync(new ObtenerSolicitudDetalleQuery
        {
            SolicitudId = id,
            EmpleadoSolicitanteId = empleadoId,
        });

        return View(new AprobarRechazarViewModel
        {
            SolicitudId = id,
            Solicitud = resultado.Solicitud,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(Guid id)
    {
        var aprobadorEmpleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("El empleado aprobador no está vinculado.");

        try
        {
            await _aprobarHandler.HandleAsync(new AprobarSolicitudCommand
            {
                SolicitudId = id,
                AprobadorEmpleadoId = aprobadorEmpleadoId,
            });

            TempData["Exito"] = "Solicitud aprobada.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = MensajeError(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(Guid id, AprobarRechazarViewModel model)
    {
        var aprobadorEmpleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("El empleado aprobador no está vinculado.");

        try
        {
            await _rechazarHandler.HandleAsync(new RechazarSolicitudCommand
            {
                SolicitudId = id,
                AprobadorEmpleadoId = aprobadorEmpleadoId,
                Comentario = model.Comentario ?? string.Empty,
            });

            TempData["Exito"] = "Solicitud rechazada.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = MensajeError(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarAprobada(Guid id)
    {
        var aprobadorEmpleadoId = _usuario.EmpleadoId
            ?? throw new InvalidOperationException("El empleado aprobador no está vinculado.");

        try
        {
            await _cancelarAprobadaHandler.HandleAsync(new CancelarAprobadaCommand
            {
                SolicitudId = id,
                AprobadorEmpleadoId = aprobadorEmpleadoId,
            });

            TempData["Exito"] = "Solicitud aprobada cancelada.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = MensajeError(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    private static string MensajeError(Exception ex)
    {
        return ex switch
        {
            SaldoInsuficienteException => "Saldo insuficiente para aprobar.",
            TransicionEstadoInvalidaException => "La operación no es válida para el estado actual de la solicitud.",
            SolicitudNoEncontradaException => "La solicitud no existe.",
            AprobadorInactivoException => "El aprobador está inactivo.",
            AutoAprobacionNoPermitidaException => "No se puede aprobar la propia solicitud.",
            _ => "Ocurrió un error al procesar la operación.",
        };
    }
}