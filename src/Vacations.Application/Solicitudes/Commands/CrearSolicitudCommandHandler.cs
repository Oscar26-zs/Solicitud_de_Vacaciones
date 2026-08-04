using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Solicitudes.Commands;

public sealed class CrearSolicitudCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;
    private readonly IRepositorioSaldoEmpleado _repositorioSaldos;
    private readonly IRepositorioHistorialSolicitud _repositorioHistorial;
    private readonly IRepositorioEmpleado _repositorioEmpleados;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CrearSolicitudCommandHandler(
        IRepositorioSolicitudVacaciones repositorioSolicitudes,
        IRepositorioSaldoEmpleado repositorioSaldos,
        IRepositorioHistorialSolicitud repositorioHistorial,
        IRepositorioEmpleado repositorioEmpleados,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _repositorioSolicitudes = repositorioSolicitudes;
        _repositorioSaldos = repositorioSaldos;
        _repositorioHistorial = repositorioHistorial;
        _repositorioEmpleados = repositorioEmpleados;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> HandleAsync(CrearSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        var ahora = _timeProvider.GetUtcNow().DateTime;
        var fechaActual = DateOnly.FromDateTime(ahora);

        var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(command.EmpleadoId, cancellationToken)
            ?? throw new InvalidOperationException($"Empleado con Id '{command.EmpleadoId}' no encontrado.");

        var rangoFechas = RangoFechas.Crear(command.FechaInicio, command.FechaFin, fechaActual);
        var diasRequeridos = rangoFechas.CalcularDiasHabiles();

        var saldo = await _repositorioSaldos.ObtenerPorEmpleadoIdAsync(command.EmpleadoId, cancellationToken)
            ?? throw new InvalidOperationException($"Saldo del empleado '{command.EmpleadoId}' no encontrado.");

        if (diasRequeridos > saldo.SaldoDisponible)
        {
            throw new SaldoInsuficienteException(saldo.SaldoDisponible, diasRequeridos);
        }

        var existeTraslape = await _repositorioSolicitudes.ExisteTraslapeAsync(
            command.EmpleadoId,
            command.FechaInicio,
            command.FechaFin,
            cancellationToken: cancellationToken);

        if (existeTraslape)
        {
            throw new TraslapeSolicitudesException();
        }

        var solicitud = SolicitudVacaciones.Crear(
            command.EmpleadoId,
            rangoFechas,
            command.Motivo,
            ahora);

        saldo.CongelarSaldo(diasRequeridos, ahora);

        var historial = HistorialSolicitud.CrearParaCreacion(
            solicitud.Id,
            empleado.Email,
            ahora);

        await _repositorioSolicitudes.AgregarAsync(solicitud, cancellationToken);
        _repositorioSaldos.Actualizar(saldo);
        await _repositorioHistorial.AgregarAsync(historial, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return solicitud.Id;
    }
}
