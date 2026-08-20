using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Exceptions;

namespace Vacations.Web.Controllers.Api;

/// <summary>
/// API JSON consumida por el agente de IA (Oro-Agente). Deliberadamente
/// separada de SolicitudVacacionesController (que sigue sirviendo vistas MVC
/// autenticadas por cookie): esta hereda de ControllerBase, así que no puede
/// devolver una vista HTML aunque un handler no esté cubierto por un catch.
/// </summary>
[ApiController]
[Route("api/vacaciones")]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public sealed class VacacionesApiController : ControllerBase
{
    private readonly CrearSolicitudCommandHandler _crearHandler;
    private readonly IValidator<CrearSolicitudCommand> _crearValidator;
    private readonly ObtenerSolicitudDetalleQueryHandler _detalleHandler;
    private readonly ILogger<VacacionesApiController> _logger;

    public VacacionesApiController(
        CrearSolicitudCommandHandler crearHandler,
        IValidator<CrearSolicitudCommand> crearValidator,
        ObtenerSolicitudDetalleQueryHandler detalleHandler,
        ILogger<VacacionesApiController> logger)
    {
        _crearHandler = crearHandler;
        _crearValidator = crearValidator;
        _detalleHandler = detalleHandler;
        _logger = logger;
    }

    [HttpPost("solicitar")]
    public async Task<IActionResult> Solicitar([FromBody] SolicitarVacacionesRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Agente IA: creando solicitud de vacaciones para empleado {EmpleadoId}",
            request.EmpleadoId);

        // El dominio no tiene un campo "destino" propio (ver TAREAS.md Fase 2):
        // se guarda dentro de Motivo con un prefijo fijo para cumplir la longitud
        // mínima de 10 caracteres que exige CrearSolicitudCommandValidator.
        var motivo = $"Viaje a {request.Destino}";
        var command = new CrearSolicitudCommand(request.EmpleadoId, request.FechaInicio, request.FechaFin, motivo);

        var validationResult = await _crearValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ErrorResponse(validationResult.Errors[0].ErrorMessage));
        }

        try
        {
            var solicitudId = await _crearHandler.HandleAsync(command, cancellationToken);
            return Ok(new SolicitarVacacionesResponse(solicitudId, "pendiente"));
        }
        catch (SaldoInsuficienteException ex)
        {
            return Conflict(new ErrorResponse(ex.Message));
        }
        catch (TraslapeSolicitudesException ex)
        {
            return Conflict(new ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            // Empleado o saldo no encontrado en el sistema (ver CrearSolicitudCommandHandler).
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            // RangoFechas.Crear valida el rango de fechas (incluye el horizonte de 2
            // meses aplicado sobre FechaFin, que CrearSolicitudCommandValidator no
            // cubre porque él valida FechaInicio) y lanza ArgumentException si falla.
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado creando solicitud de vacaciones para empleado {EmpleadoId}", request.EmpleadoId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Ocurrió un error inesperado procesando la solicitud."));
        }
    }

    [HttpGet("{solicitudId:guid}/estado")]
    public async Task<IActionResult> Estado(Guid solicitudId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Agente IA: consultando estado de la solicitud {SolicitudId}", solicitudId);

        try
        {
            // Este endpoint no recibe empleadoId (el contrato con el agente de IA
            // no lo incluye): la confianza se establece a nivel de transporte vía
            // ApiKeyAuthFilter. Se piden EsAprobador/EsRRHH = true para reusar
            // ObtenerSolicitudDetalleQueryHandler sin duplicar su lógica de acceso,
            // en vez de exigir que el llamador sea el dueño de la solicitud.
            var query = new ObtenerSolicitudDetalleQuery(solicitudId, Guid.Empty, EsAprobador: true, EsRRHH: true);
            var solicitud = await _detalleHandler.HandleAsync(query, cancellationToken);

            var estado = EstadoSolicitudMapeador.AEstadoApi(solicitud.Estado);
            return Ok(new EstadoSolicitudResponse(solicitud.Id, estado));
        }
        catch (SolicitudNoEncontradaException)
        {
            return NotFound(new ErrorResponse("No se encontró la solicitud indicada."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado consultando el estado de la solicitud {SolicitudId}", solicitudId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Ocurrió un error inesperado consultando la solicitud."));
        }
    }
}
