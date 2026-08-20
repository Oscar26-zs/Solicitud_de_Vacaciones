using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Vacations.Web.Controllers;

namespace Vacations.Web.Tests.Controllers;

public class ChatControllerTests
{
    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private static ChatController CrearController(HttpMessageHandler handler, Guid? empleadoId)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:8001") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("OroAgente").Returns(httpClient);

        var controller = new ChatController(factory, Substitute.For<ILogger<ChatController>>());

        var claims = new List<Claim>();
        if (empleadoId.HasValue)
        {
            claims.Add(new Claim("EmpleadoId", empleadoId.Value.ToString()));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) }
        };

        return controller;
    }

    private static bool LeerOk(object value) => (bool)value.GetType().GetProperty("ok")!.GetValue(value)!;

    private static string LeerRespuesta(object value) => (string)value.GetType().GetProperty("respuesta")!.GetValue(value)!;

    [Fact]
    public async Task Enviar_MensajeVacio_NoLlamaAlServicioYDevuelveError()
    {
        var handlerLlamado = false;
        var handler = new StubHttpMessageHandler(_ =>
        {
            handlerLlamado = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var controller = CrearController(handler, Guid.NewGuid());

        var resultado = await controller.Enviar("   ", CancellationToken.None);

        Assert.False(handlerLlamado);
        var json = Assert.IsType<JsonResult>(resultado);
        Assert.False(LeerOk(json.Value!));
    }

    [Fact]
    public async Task Enviar_SinEmpleadoIdEnLaSesion_DevuelveErrorSinLlamarAlServicio()
    {
        var handlerLlamado = false;
        var handler = new StubHttpMessageHandler(_ =>
        {
            handlerLlamado = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var controller = CrearController(handler, empleadoId: null);

        var resultado = await controller.Enviar("Hola", CancellationToken.None);

        Assert.False(handlerLlamado);
        var json = Assert.IsType<JsonResult>(resultado);
        Assert.False(LeerOk(json.Value!));
    }

    [Fact]
    public async Task Enviar_RespuestaExitosa_ReenviaLaRespuestaDelAgente()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"respuesta\": \"Tu solicitud fue creada.\"}", Encoding.UTF8, "application/json")
        });
        var controller = CrearController(handler, Guid.NewGuid());

        var resultado = await controller.Enviar("Quiero vacaciones", CancellationToken.None);

        var json = Assert.IsType<JsonResult>(resultado);
        Assert.True(LeerOk(json.Value!));
        Assert.Equal("Tu solicitud fue creada.", LeerRespuesta(json.Value!));
    }

    [Fact]
    public async Task Enviar_ServicioRespondeError_DevuelveMensajeAmigable()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var controller = CrearController(handler, Guid.NewGuid());

        var resultado = await controller.Enviar("Hola", CancellationToken.None);

        var json = Assert.IsType<JsonResult>(resultado);
        Assert.False(LeerOk(json.Value!));
    }

    [Fact]
    public async Task Enviar_ErrorDeConexion_DevuelveMensajeAmigableSinLanzar()
    {
        var handler = new StubHttpMessageHandler(_ => throw new HttpRequestException("boom"));
        var controller = CrearController(handler, Guid.NewGuid());

        var resultado = await controller.Enviar("Hola", CancellationToken.None);

        var json = Assert.IsType<JsonResult>(resultado);
        Assert.False(LeerOk(json.Value!));
    }
}
