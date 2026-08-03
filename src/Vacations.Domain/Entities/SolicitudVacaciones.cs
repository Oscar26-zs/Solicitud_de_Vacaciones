using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Domain.Entities;

/// <summary>
/// Entidad central que encapsula el ciclo de vida de una solicitud de
/// vacaciones, incluyendo la máquina de estados y las transiciones válidas
/// (constitution §2). Los estados finales son inmutables salvo
/// <c>Approved → Cancelled</c> antes del inicio del periodo.
/// </summary>
public sealed class SolicitudVacaciones
{
    public const int MotivoMinLength = 10;
    public const int MotivoMaxLength = 1000;
    public const int ComentarioMaxLength = 500;

    public Guid Id { get; private set; }

    public Guid EmpleadoId { get; private set; }

    public DateTime FechaInicio { get; private set; }

    public DateTime FechaFin { get; private set; }

    public int DiasRequeridos { get; private set; }

    public EstadoSolicitud Estado { get; private set; }

    public string Motivo { get; private set; }

    public string? ComentarioAprobador { get; private set; }

    public Guid? AprobadoPor { get; private set; }

    public DateTime CreadoEn { get; private set; }

    public DateTime ActualizadoEn { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private SolicitudVacaciones(
        Guid id,
        Guid empleadoId,
        DateTime fechaInicio,
        DateTime fechaFin,
        int diasRequeridos,
        string motivo,
        DateTime creadoEn)
    {
        Id = id;
        EmpleadoId = empleadoId;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        DiasRequeridos = diasRequeridos;
        Estado = EstadoSolicitud.Pending;
        Motivo = motivo;
        CreadoEn = creadoEn;
        ActualizadoEn = creadoEn;
    }

    public static SolicitudVacaciones Crear(Guid empleadoId, RangoFechas rango, string motivo, DateTime fechaActual)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("El motivo es obligatorio", nameof(motivo));
        }

        if (motivo.Trim().Length < MotivoMinLength)
        {
            throw new ArgumentException($"El motivo debe tener al menos {MotivoMinLength} caracteres", nameof(motivo));
        }

        if (motivo.Trim().Length > MotivoMaxLength)
        {
            throw new ArgumentException($"El motivo no puede exceder {MotivoMaxLength} caracteres", nameof(motivo));
        }

        var dias = rango.CalcularDiasHabiles();
        if (dias < 1)
        {
            throw new ArgumentException("La solicitud debe tener al menos 1 día hábil", nameof(rango));
        }

        return new SolicitudVacaciones(
            Guid.NewGuid(),
            empleadoId,
            rango.FechaInicio,
            rango.FechaFin,
            dias,
            motivo.Trim(),
            fechaActual.Date);
    }

    public void Aprobar(Guid aprobadorId, DateTime fechaActual)
    {
        if (aprobadorId == EmpleadoId)
        {
            throw new AutoAprobacionNoPermitidaException();
        }

        if (Estado != EstadoSolicitud.Pending)
        {
            throw new TransicionEstadoInvalidaException($"Solo se puede aprobar una solicitud Pending (actual: {Estado})");
        }

        Estado = EstadoSolicitud.Approved;
        AprobadoPor = aprobadorId;
        ActualizadoEn = fechaActual.Date;
    }

    public void Rechazar(Guid aprobadorId, string comentario, DateTime fechaActual)
    {
        if (aprobadorId == EmpleadoId)
        {
            throw new AutoAprobacionNoPermitidaException();
        }

        if (string.IsNullOrWhiteSpace(comentario))
        {
            throw new ArgumentException("El comentario es obligatorio", nameof(comentario));
        }

        if (comentario.Trim().Length > ComentarioMaxLength)
        {
            throw new ArgumentException($"El comentario no puede exceder {ComentarioMaxLength} caracteres", nameof(comentario));
        }

        if (Estado != EstadoSolicitud.Pending)
        {
            throw new TransicionEstadoInvalidaException($"Solo se puede rechazar una solicitud en Pending (estado: {Estado})");
        }

        Estado = EstadoSolicitud.Rejected;
        ComentarioAprobador = comentario.Trim();
        AprobadoPor = aprobadorId;
        ActualizadoEn = fechaActual.Date;
    }

    public void Cancelar(DateTime fechaActual)
    {
        if (Estado != EstadoSolicitud.Pending)
        {
            throw new TransicionEstadoInvalidaException($"No se puede cancelar una solicitud en estado {Estado}");
        }

        Estado = EstadoSolicitud.Cancelled;
        ActualizadoEn = fechaActual.Date;
    }

    /// <summary>
    /// Cancela una solicitud aprobada antes de que inicie el periodo.
    /// Solo puede cancelar un aprobador y solo si la fecha de inicio es futura (RN-04, RF-047).
    /// </summary>
    public void CancelarAprobada(DateTime fechaActual)
    {
        if (Estado != EstadoSolicitud.Approved)
        {
            throw new TransicionEstadoInvalidaException($"Solo se puede cancelar una solicitud Approved (estado: {Estado})");
        }

        if (FechaInicio <= fechaActual.Date)
        {
            throw new TransicionEstadoInvalidaException("No se puede cancelar: el periodo de vacaciones ya ha iniciado");
        }

        Estado = EstadoSolicitud.Cancelled;
        ActualizadoEn = fechaActual.Date;
    }

    public void Expirar(DateTime fechaActual)
    {
        if (Estado != EstadoSolicitud.Pending)
        {
            throw new TransicionEstadoInvalidaException($"Solo puede expirar una solicitud en Pending (estado: {Estado})");
        }

        Estado = EstadoSolicitud.Expired;
        ActualizadoEn = fechaActual.Date;
    }

    public void Editar(RangoFechas nuevoRango, string nuevoMotivo, int nuevosDias, DateTime fechaActual)
    {
        if (Estado != EstadoSolicitud.Pending)
        {
            throw new TransicionEstadoInvalidaException("Solo se pueden editar solicitudes pendientes");
        }

        if (string.IsNullOrWhiteSpace(nuevoMotivo) || nuevoMotivo.Trim().Length < MotivoMinLength)
        {
            throw new ArgumentException($"El motivo debe tener al menos {MotivoMinLength} caracteres", nameof(nuevoMotivo));
        }

        FechaInicio = nuevoRango.FechaInicio;
        FechaFin = nuevoRango.FechaFin;
        Motivo = nuevoMotivo.Trim();
        DiasRequeridos = nuevosDias;
        ActualizadoEn = fechaActual.Date;
    }
}