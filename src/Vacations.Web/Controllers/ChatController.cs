using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Vacations.Web.Controllers;

/// <summary>
/// Proxy hacia el servicio de IA (Oro-Agente, FastAPI) que atiende el chat de
/// vacaciones. El navegador nunca habla directamente con ese servicio: este
/// controller toma el EmpleadoId de la sesión ya autenticada (no confía en lo
/// que mande el cliente) y reenvía el mensaje, igual que el resto de acciones
/// AJAX de este proyecto (ver SolicitudVacacionesController).
/// </summary>
[Authorize]
public sealed class ChatController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IHttpClientFactory httpClientFactory, ILogger<ChatController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private Guid ObtenerEmpleadoId()
    {
        var claim = User.FindFirst("EmpleadoId")?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enviar([FromForm] string mensaje, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            return Json(new { ok = false, respuesta = "Escribe un mensaje antes de enviarlo." });
        }

        var empleadoId = ObtenerEmpleadoId();
        if (empleadoId == Guid.Empty)
        {
            return Json(new { ok = false, respuesta = "No se pudo identificar tu usuario. Intenta iniciar sesión de nuevo." });
        }

        var client = _httpClientFactory.CreateClient("OroAgente");

        try
        {
            var response = await client.PostAsJsonAsync(
                "/chat",
                new { mensaje, empleadoId = empleadoId.ToString() },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("El servicio de chat de IA respondió {StatusCode}", response.StatusCode);
                return Json(new { ok = false, respuesta = "El asistente no está disponible en este momento. Intenta de nuevo más tarde." });
            }

            var payload = await response.Content.ReadFromJsonAsync<ChatRespuestaDto>(cancellationToken: cancellationToken);
            return Json(new { ok = true, respuesta = payload?.Respuesta ?? "No obtuve una respuesta del asistente." });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "No se pudo conectar con el servicio de chat de IA");
            return Json(new { ok = false, respuesta = "No se pudo conectar con el asistente de vacaciones. Verifica que el servicio esté disponible." });
        }
    }

    private sealed record ChatRespuestaDto([property: JsonPropertyName("respuesta")] string Respuesta);
}
