using FluentValidation;
using Vacations.Application.Common;
using Vacations.Domain.Abstractions;
using Vacations.Domain.Entities;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>
/// Handler del caso de uso CU-04: crea una solicitud validando saldo y traslape,
/// congela el saldo pendiente y registra la solicitud junto con su historial.
/// </summary>
public sealed class CrearSolicitudCommandHandler
{
    private readonly IRepositorioSolicitudVacaciones _solicitudes;
    private readonly IRepositorioSaldoEmpleado _saldos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProveedorTiempoCorporativo _proveedorTiempo;
    private readonly CrearSolicitudCommandValidator _validator;

    public CrearSolicitudCommandHandler(
        IRepositorioSolicitudVacaciones solicitudes,
        IRepositorioSaldoEmpleado saldos,
        IUnitOfWork unitOfWork,
        IProveedorTiempoCorporativo proveedorTiempo,
        CrearSolicitudCommandValidator validator)
    {
        _solicitudes = solicitudes;
        _saldos = saldos;
        _unitOfWork = unitOfWork;
        _proveedorTiempo = proveedorTiempo;
        _validator = validator;
    }

    public async Task<CrearSolicitudResult> HandleAsync(
        CrearSolicitudCommand comando,
        CancellationToken cancellationToken = default)
    {
        var fechaActual = _proveedorTiempo.ObtenerFechaActualCorporativa();

        var validacion = await _validator.ValidateAsync(comando, cancellationToken);
        if (!validacion.IsValid)
        {
            throw new ValidationException(validacion.Errors);
        }

        var rango = RangoFechas.Crear(comando.FechaInicio, comando.FechaFin, fechaActual);
        var saldo = await _saldos.ObtenerPorEmpleadoIdAsync(comando.EmpleadoId, cancellationToken)
            ?? throw new SaldoNoEncontradoException("No se encontró un saldo registrado para el empleado.");

        if (saldo.SaldoDisponible < rango.CalcularDiasHabiles())
        {
            throw new SaldoInsuficienteException();
        }

        var hayTraslape = await _solicitudes.ExisteTraslapeAsync(
            comando.EmpleadoId,
            rango.FechaInicio,
            rango.FechaFin,
            excluirSolicitudId: null,
            cancellationToken);

        if (hayTraslape)
        {
            throw new TraslapeSolicitudesException();
        }

        var solicitud = SolicitudVacaciones.Crear(
            comando.EmpleadoId,
            rango,
            comando.Motivo,
            fechaActual);

        saldo.CongelarSaldo(solicitud.DiasRequeridos, fechaActual);

        await _solicitudes.AgregarAsync(solicitud, cancellationToken);
        await _saldos.ActualizarAsync(saldo, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CrearSolicitudResult(solicitud.Id);
    }
}