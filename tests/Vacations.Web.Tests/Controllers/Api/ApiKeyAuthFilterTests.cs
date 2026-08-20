using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Vacations.Web.Controllers.Api;

namespace Vacations.Web.Tests.Controllers.Api;

public class ApiKeyAuthFilterTests
{
    private static ActionExecutingContext CrearContexto(string? apiKeyEnviada)
    {
        var httpContext = new DefaultHttpContext();
        if (apiKeyEnviada is not null)
        {
            httpContext.Request.Headers["X-Api-Key"] = apiKeyEnviada;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: new object());
    }

    private static IConfiguration CrearConfiguracion(string? apiKeyConfigurada)
    {
        var configuracion = Substitute.For<IConfiguration>();
        configuracion["AgenteIA:ApiKey"].Returns(apiKeyConfigurada);
        return configuracion;
    }

    [Fact]
    public async Task ApiKeyValida_PermiteContinuar()
    {
        var filtro = new ApiKeyAuthFilter(CrearConfiguracion("clave-secreta"));
        var contexto = CrearContexto("clave-secreta");
        var siguienteFueLlamado = false;

        await filtro.OnActionExecutionAsync(contexto, () =>
        {
            siguienteFueLlamado = true;
            return Task.FromResult(new ActionExecutedContext(contexto, contexto.Filters, contexto.Controller));
        });

        Assert.True(siguienteFueLlamado);
        Assert.Null(contexto.Result);
    }

    [Fact]
    public async Task ApiKeyAusente_Devuelve401YNoContinua()
    {
        var filtro = new ApiKeyAuthFilter(CrearConfiguracion("clave-secreta"));
        var contexto = CrearContexto(apiKeyEnviada: null);
        var siguienteFueLlamado = false;

        await filtro.OnActionExecutionAsync(contexto, () =>
        {
            siguienteFueLlamado = true;
            return Task.FromResult(new ActionExecutedContext(contexto, contexto.Filters, contexto.Controller));
        });

        Assert.False(siguienteFueLlamado);
        var resultado = Assert.IsType<ObjectResult>(contexto.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, resultado.StatusCode);
    }

    [Fact]
    public async Task ApiKeyIncorrecta_Devuelve401()
    {
        var filtro = new ApiKeyAuthFilter(CrearConfiguracion("clave-secreta"));
        var contexto = CrearContexto("clave-equivocada");

        await filtro.OnActionExecutionAsync(contexto, () =>
            Task.FromResult(new ActionExecutedContext(contexto, contexto.Filters, contexto.Controller)));

        var resultado = Assert.IsType<ObjectResult>(contexto.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, resultado.StatusCode);
    }

    [Fact]
    public async Task SinApiKeyConfiguradaEnElServidor_Devuelve503()
    {
        var filtro = new ApiKeyAuthFilter(CrearConfiguracion(apiKeyConfigurada: null));
        var contexto = CrearContexto("cualquier-cosa");

        await filtro.OnActionExecutionAsync(contexto, () =>
            Task.FromResult(new ActionExecutedContext(contexto, contexto.Filters, contexto.Controller)));

        var resultado = Assert.IsType<ObjectResult>(contexto.Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, resultado.StatusCode);
    }
}
