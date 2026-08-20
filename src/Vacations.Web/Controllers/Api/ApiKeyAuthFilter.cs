using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Vacations.Web.Controllers.Api;

/// <summary>
/// Autenticación servidor-a-servidor para el agente de IA: no hay sesión de
/// navegador, así que en vez de [Authorize] + cookie de Identity se exige un
/// header X-Api-Key que coincida con AgenteIA:ApiKey. Ver TAREAS.md, Fase 4.
/// </summary>
public sealed class ApiKeyAuthFilter : IAsyncActionFilter
{
    private const string HeaderName = "X-Api-Key";

    private readonly IConfiguration _configuration;

    public ApiKeyAuthFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var apiKeyConfigurada = _configuration["AgenteIA:ApiKey"];

        if (string.IsNullOrEmpty(apiKeyConfigurada))
        {
            context.Result = new ObjectResult(
                new ErrorResponse("El servidor no tiene configurada AgenteIA:ApiKey; este endpoint está deshabilitado."))
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
            return;
        }

        var apiKeyRecibida = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrEmpty(apiKeyRecibida) || apiKeyRecibida != apiKeyConfigurada)
        {
            context.Result = new ObjectResult(new ErrorResponse("API key inválida o ausente."))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        await next();
    }
}
