using System.Text.Json;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>
/// Handler del caso de uso CU-06: edita fechas/motivo de una solicitud Pending,
/// recalcula días, ajusta el saldo pendiente y registra los cambios en historial.
/// </summary>
public sealed class EditarSolicitudCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;
    private readonly IRepositorioSaldoEmpleado _saldos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public EditarSolicitudCommandHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioSaldoEmpleado saldos,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _solicitudes = solicitudes;
        _saldos = saldos;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task HandleAsync(EditarSolicitudCommand comando, CancellationToken cancellationToken = default)
    {
        var fechaActual = _timeProvider.GetUtcNow().UtcDateTime.Date;

        var solicitud = await _solicitudes.ObtenerPorIdAsync(comando.SolicitudId, cancellationToken)
            ?? throw new SolicitudNoEncontradaException();

        if (solicitud.EmpleadoId != comando.EmpleadoId)
        {
            throw new AccesoNoPermitidoException("No se puede editar una solicitud de otro empleado.");
        }

        var nuevoRango = RangoFechas.Crear(comando.FechaInicio, comando.FechaFin, fechaActual);
        var nuevosDias = nuevoRango.CalcularDiasHabiles();

        var hayTraslape = await _solicitudes.ExisteTraslapeAsync(
            comando.EmpleadoId,
            nuevoRango.FechaInicio,
            nuevoRango.FechaFin,
            solicitud.Id,
            cancellationToken);

        if (hayTraslape)
        {
            throw new TraslapeSolicitudesException();
        }

        var saldo = await _saldos.ObtenerPorEmpleadoIdAsync(comando.EmpleadoId, cancellationToken)
            ?? throw new SaldoNoEncontradoException("No se encontró un saldo registrado para el empleado.");

        var diferenciaDias = nuevosDias - solicitud.DiasRequeridos;
        if (diferenciaDias > 0)
        {
            saldo.CongelarSaldo(diferenciaDias, fechaActual);
        }
        else if (diferenciaDias < 0)
        {
            saldo.LiberarSaldoPendiente(Math.Abs(diferenciaDias), fechaActual);
        }

        var camposModificados = ObtenerCamposModificados(solicitud, comando);
        solicitud.Editar(nuevoRango, comando.Motivo, nuevosDias, fechaActual);

        await _solicitudes.ActualizarAsync(solicitud, cancellationToken);
        await _saldos.ActualizarAsync(saldo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string ObtenerCamposModificados(SolicitudVacaciones solicitud, EditarSolicitudCommand comando)
    {
        var cambios = new Dictionary<string, object>();

        if (solicitud.FechaInicio != comando.FechaInicio)
        {
            cambios["FechaInicio"] = new { old = solicitud.FechaInicio, @new = comando.FechaInicio };
        }

        if (solicitud.FechaFin != comando.FechaFin)
        {
            cambios["FechaFin"] = new { old = solicitud.FechaFin, @new = comando.FechaFin };
        }

        if (solicitud.Motivo != comando.Motivo)
        {
            cambios["Motivo"] = new { old = solicitud.Motivo, @new = comando.Motivo };
        }

        return cambios.Count > 0
            ? JsonSerializer.Serialize(cambios)
            : string.Empty;
    }
}