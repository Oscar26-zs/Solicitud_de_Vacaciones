using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vacations.Infrastructure.Persistence;

namespace Vacations.Web.Tests;

/// <summary>
/// Factory para pruebas de integración. Utiliza una base SQL Server dedicada
/// (`VacacionesDbTest`) que se migra al arrancar, para no tocar datos de desarrollo.
/// </summary>
public class VacacionesWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string ConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=VacacionesDbTest;Trusted_Connection=True;MultipleActiveResultSets=true";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:VacacionesDb", ConnectionString);

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:VacacionesDb"] = ConnectionString,
            });
        });

        builder.ConfigureServices(services =>
        {
            var hosted = services
                .Where(d => d.ServiceType.FullName?.Contains("BackgroundService") == true)
                .ToList();
            foreach (var h in hosted)
            {
                services.Remove(h);
            }
        });
    }

    public async Task InicializarBaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VacacionesDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }
}