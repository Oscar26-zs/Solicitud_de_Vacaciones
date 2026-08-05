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
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Demasiadas solicitudes. Intente de nuevo más tarde.", token);
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
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
