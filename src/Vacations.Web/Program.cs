using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Vacations.Application;
using Vacations.Infrastructure;
using Vacations.Infrastructure.Data;
using Vacations.Infrastructure.Identity;
using Vacations.Infrastructure.Persistence;
using Vacations.Web.Authorization;

var builder = WebApplication.CreateBuilder(args);

var httpsPort = builder.Configuration["HTTPS_PORT"] ?? builder.Configuration["ASPNETCORE_HTTPS_PORT"];

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Politicas.RequiereEmpleado, policy =>
        policy.RequireRole(Roles.Empleado, Roles.Aprobador, Roles.RRHH));

    options.AddPolicy(Politicas.RequiereAprobador, policy =>
        policy.RequireRole(Roles.Aprobador));

    options.AddPolicy(Politicas.RequiereRRHH, policy =>
        policy.RequireRole(Roles.RRHH));
});

builder.Services.AddRateLimiter(options =>
{
    // Límites documentados en plan.md (sección "Objetivos de rendimiento"):
    //   - Lectura   (GET: listar, paginar, filtrar, detalle, saldo): 120/min por usuario
    //   - Escritura (POST/PUT/PATCH/DELETE: crear, editar, aprobar, rechazar, cancelar): 30/min por usuario
    //   - Auth      (login): 10/min por IP
    const int limiteLecturaPorMinuto = 120;
    const int limiteEscrituraPorMinuto = 30;
    const int limiteAuthPorMinuto = 10;

    // Política específica para el login (endpoint público): se cuenta por IP.
    options.AddPolicy<string>("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anónimo",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limiteAuthPorMinuto,
                Window = TimeSpan.FromMinutes(1)
            }));

    // IMPORTANTE: este middleware se registra DESPUÉS de UseAuthentication() (ver más abajo),
    // porque la clave de partición depende de la identidad del usuario autenticado.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var claveUsuario = context.User.Identity?.Name
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "anónimo";

        var esEscritura = HttpMethods.IsPost(context.Request.Method)
            || HttpMethods.IsPut(context.Request.Method)
            || HttpMethods.IsPatch(context.Request.Method)
            || HttpMethods.IsDelete(context.Request.Method);

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{(esEscritura ? "escritura" : "lectura")}:{claveUsuario}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = esEscritura ? limiteEscrituraPorMinuto : limiteLecturaPorMinuto,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "60";

        var esAjax = context.HttpContext.Request.Headers.Accept.ToString().Contains("application/json")
            || context.HttpContext.Request.Headers.XRequestedWith.ToString().Contains("XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (esAjax)
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = "Has realizado demasiadas solicitudes. Espera unos segundos antes de reintentar.",
                message = "Has realizado demasiadas solicitudes. Espera unos segundos antes de reintentar."
            }, token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync("Demasiadas solicitudes. Intente de nuevo más tarde.", token);
        }
    };
});

var app = builder.Build();

// Apply pending migrations in all environments to ensure the database exists
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VacacionesDbContext>();
    await SeedData.ApplyMigrationsAsync(dbContext);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UsuarioAplicacion>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    await SeedData.InitializeAsync(dbContext, userManager, roleManager);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!string.IsNullOrWhiteSpace(httpsPort))
{
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseRouting();
app.UseAuthentication();
// El rate limiter va DESPUÉS de UseAuthentication() para que la clave de
// partición use la identidad del usuario y no caiga a la IP en sesiones autenticadas.
app.UseRateLimiter();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
