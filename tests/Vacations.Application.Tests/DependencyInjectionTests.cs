using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Vacations.Application;
using Vacations.Application.Saldos.Commands;
using Vacations.Application.Saldos.Queries;
using Vacations.Application.Solicitudes.Commands;
using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Abstractions;

namespace Vacations.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationServices_RegistraHandlersYQueries()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped(_ => Substitute.For<IRepositorioSolicitudVacaciones>());
        services.AddScoped(_ => Substitute.For<IRepositorioSaldoEmpleado>());
        services.AddScoped(_ => Substitute.For<IRepositorioHistorialSolicitud>());
        services.AddScoped(_ => Substitute.For<IRepositorioEmpleado>());
        services.AddScoped(_ => Substitute.For<IUnitOfWork>());

        // Act
        services.AddApplicationServices();
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<CrearSolicitudCommandHandler>());
        Assert.NotNull(provider.GetService<EditarSolicitudCommandHandler>());
        Assert.NotNull(provider.GetService<CancelarSolicitudCommandHandler>());
        Assert.NotNull(provider.GetService<AprobarSolicitudCommandHandler>());
        Assert.NotNull(provider.GetService<RechazarSolicitudCommandHandler>());
        Assert.NotNull(provider.GetService<CancelarAprobadaCommandHandler>());
        Assert.NotNull(provider.GetService<ObtenerMisSolicitudesQueryHandler>());
        Assert.NotNull(provider.GetService<ObtenerSolicitudDetalleQueryHandler>());
        Assert.NotNull(provider.GetService<ObtenerBandejaAprobadorQueryHandler>());
        Assert.NotNull(provider.GetService<ObtenerSolicitudesRRHHQueryHandler>());
        Assert.NotNull(provider.GetService<ObtenerSaldoQueryHandler>());
        Assert.NotNull(provider.GetService<AcumularSaldoMensualCommandHandler>());
        Assert.NotNull(provider.GetService<CrearSolicitudCommandValidator>());
        Assert.NotNull(provider.GetService<RechazarSolicitudCommandValidator>());

        var fluentValidators = services.Where(s => s.ServiceType.Name.Contains("Validator")).ToList();
        Assert.NotEmpty(fluentValidators);
    }
}