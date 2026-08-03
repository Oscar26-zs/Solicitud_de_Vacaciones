using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vacations.Web.Tests;

public sealed class SolicitudVacacionesControllerTests : IClassFixture<VacacionesWebApplicationFactory>
{
    private readonly VacacionesWebApplicationFactory _factory;

    public SolicitudVacacionesControllerTests(VacacionesWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.InicializarBaseAsync().GetAwaiter().GetResult();
    }

    private HttpClient CrearClienteConRol(string rol)
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.Scheme)
                    .AddScheme<TestAuthOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        return client;
    }

    [Fact]
    public async Task UsuarioNoAutenticado_RedirigeAlLogin()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/SolicitudVacaciones");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Cuenta/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task EmpleadoAutenticado_PuedeVerSuDashboard()
    {
        var client = CrearClienteConRol("Empleado");
        client.DefaultRequestHeaders.Add("X-Test-EmpleadoId", "11111111-1111-1111-1111-111111111111");
        client.DefaultRequestHeaders.Add("X-Test-Email", "empleado@test.com");

        var response = await client.GetAsync("/SolicitudVacaciones");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Mis solicitudes", content);
    }

    [Fact]
    public async Task Empleado_PuedeCrearSolicitud()
    {
        var client = CrearClienteConRol("Empleado");
        client.DefaultRequestHeaders.Add("X-Test-Empleado", "22222222-2222-2222-2222-222222222222");
        client.DefaultRequestHeaders.Add("X-Test-Rol", "Empleado");

        var response = await client.GetAsync("/SolicitudVacaciones/Crear");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Nueva solicitud", content);
    }

    [Fact]
    public async Task Empleado_NoPuedeAccederABandejaAprobador()
    {
        var client = CrearClienteConRol("Empleado");
        client.DefaultRequestHeaders.Add("X-Test-Rol", "Empleado");

        var response = await client.GetAsync("/BandejaAprobador");

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Redirect);
    }
}

public static class TestAuthDefaults
{
    public const string Scheme = "Test";
}

public sealed class TestAuthOptions : AuthenticationSchemeOptions
{
}

/// <summary>
/// Manejador de autenticación de prueba: autentica al usuario con el rol indicado
/// en el header "X-Test-Rol" (o "Empleado" por defecto).
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
{
    public const string Scheme = "Test";

    public TestAuthHandler(
        IOptionsMonitor<TestAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var rol = Context.Request.Headers["X-Test-Rol"].FirstOrDefault() ?? "Empleado";
        var email = Context.Request.Headers["X-Test-Email"].FirstOrDefault() ?? $"{rol}@test.com";

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, rol),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        };

        // El empleado autenticado (opcional) para simular usuario vinculado.
        var empleadoId = Context.Request.Headers["X-Test-EmpleadoId"].FirstOrDefault();
        if (Guid.TryParse(empleadoId, out var empleadoIdGuid))
        {
            claims = claims.Append(new Claim("EmpleadoId", empleadoId)).ToArray();
        }

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}