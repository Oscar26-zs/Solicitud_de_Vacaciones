using System.Text.Json;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Solicitudes.Commands;

public sealed class EditarSolicitudCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _repositorioSolicitudes;
    private readonly IRepositorioSaldoEmpleado _repositorioSaldos;
    private readonly IRepositorioHistorialSolicitud _repositorioHistorial;
    private readonly IRepositorioEmpleado _repositorioEmpleados;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public EditarSolicitudCommandHandler(
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

    public async Task HandleAsync(EditarSolicitudCommand command, CancellationToken cancellationToken = default)
    {
        var ahora = _timeProvider.GetUtcNow().DateTime;
        var fechaActual = DateOnly.FromDateTime(ahora);

        var solicitud = await _repositorioSolicitudes.ObtenerPorIdAsync(command.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException(command.SolicitudId);

        if (solicitud.EmpleadoId != command.EmpleadoId)
        {
            throw new AccesoNoAutorizadoException("No tiene permiso para editar esta solicitud.");
        }

        if (!solicitud.PuedeSerEditada())
        {
            throw new InvalidOperationException("Solo se pueden editar solicitudes en estado Pending.");
        }

        var empleado = await _repositorioEmpleados.ObtenerPorIdAsync(command.EmpleadoId, cancellationToken)
            ?? throw new InvalidOperationException($"Empleado con Id '{command.EmpleadoId}' no encontrado.");

        var nuevoRango = RangoFechas.Crear(command.FechaInicio, command.FechaFin, fechaActual);
        var nuevosDias = nuevoRango.CalcularDiasHabiles();
        var diasAnteriores = solicitud.DiasRequeridos;

        var existeTraslape = await _repositorioSolicitudes.ExisteTraslapeAsync(
            command.EmpleadoId,
            command.FechaInicio,
            command.FechaFin,
            command.SolicitudId,
            cancellationToken);

        if (existeTraslape)
        {
            throw new TraslapeSolicitudesException();
        }

        var saldo = await _repositorioSaldos.ObtenerPorEmpleadoIdAsync(command.EmpleadoId, cancellationToken)
            ?? throw new InvalidOperationException($"Saldo del empleado '{command.EmpleadoId}' no encontrado.");

        var camposModificados = new Dictionary<string, object>();

        if (solicitud.FechaInicio != command.FechaInicio)
        {
            camposModificados["FechaInicio"] = new { Anterior = solicitud.FechaInicio, Nuevo = command.FechaInicio };
        }

        if (solicitud.FechaFin != command.FechaFin)
        {
            camposModificados["FechaFin"] = new { Anterior = solicitud.FechaFin, Nuevo = command.FechaFin };
        }

        if (solicitud.Motivo != command.Motivo)
        {
            camposModificados["Motivo"] = new { Anterior = solicitud.Motivo, Nuevo = command.Motivo };
        }

        if (diasAnteriores != nuevosDias)
        {
            camposModificados["DiasRequeridos"] = new { Anterior = diasAnteriores, Nuevo = nuevosDias };
            saldo.AjustarSaldoPendiente(diasAnteriores, nuevosDias, ahora);
            _repositorioSaldos.Actualizar(saldo);
        }

        solicitud.Editar(nuevoRango, command.Motivo, ahora);

        var historial = HistorialSolicitud.CrearParaEdicion(
            solicitud.Id,
            empleado.Email,
            ahora,
            JsonSerializer.Serialize(camposModificados));

        _repositorioSolicitudes.Actualizar(solicitud);
        await _repositorioHistorial.AgregarAsync(historial, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
