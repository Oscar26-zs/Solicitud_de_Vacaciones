using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Domain.Entities;

public sealed class SolicitudVacaciones
{
    public Guid Id { get; private set; }
    public Guid EmpleadoId { get; private set; }
    public DateOnly FechaInicio { get; private set; }
    public DateOnly FechaFin { get; private set; }
    public int DiasRequeridos { get; private set; }
    public EstadoSolicitud Estado { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public string? ComentarioAprobador { get; private set; }
    public Guid? AprobadoPor { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public DateTime ActualizadoEn { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private SolicitudVacaciones()
    {
    }

    public static SolicitudVacaciones Crear(
        Guid empleadoId,
        RangoFechas rangoFechas,
        string motivo,
        DateTime fechaActual)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("El motivo no puede estar vacío.", nameof(motivo));
        }

        var diasHabiles = rangoFechas.CalcularDiasHabiles();

        return new SolicitudVacaciones
        {
            Id = Guid.NewGuid(),
            EmpleadoId = empleadoId,
            FechaInicio = rangoFechas.FechaInicio,
            FechaFin = rangoFechas.FechaFin,
            DiasRequeridos = diasHabiles,
            Estado = EstadoSolicitud.Pending,
            Motivo = motivo.Trim(),
            CreadoEn = fechaActual,
            ActualizadoEn = fechaActual
        };
    }

    public void Aprobar(Guid aprobadorId, DateTime fechaActual)
    {
        ValidarTransicion(EstadoSolicitud.Approved);
        ValidarNoAutoAprobacion(aprobadorId);

        Estado = EstadoSolicitud.Approved;
        AprobadoPor = aprobadorId;
        ActualizadoEn = fechaActual;
    }

    public void Rechazar(Guid aprobadorId, string comentario, DateTime fechaActual)
    {
        ValidarTransicion(EstadoSolicitud.Rejected);
        ValidarNoAutoAprobacion(aprobadorId);

        if (string.IsNullOrWhiteSpace(comentario))
        {
            throw new ArgumentException("El comentario es obligatorio al rechazar una solicitud.", nameof(comentario));
        }

        if (comentario.Length > 500)
        {
            throw new ArgumentException("El comentario no puede exceder los 500 caracteres.", nameof(comentario));
        }

        Estado = EstadoSolicitud.Rejected;
        AprobadoPor = aprobadorId;
        ComentarioAprobador = comentario.Trim();
        ActualizadoEn = fechaActual;
    }

    public void Cancelar(DateTime fechaActual)
    {
        ValidarTransicion(EstadoSolicitud.Cancelled);

        Estado = EstadoSolicitud.Cancelled;
        ActualizadoEn = fechaActual;
    }

    public void CancelarAprobada(Guid aprobadorId, DateOnly fechaActualDate, DateTime fechaActual)
    {
        if (Estado != EstadoSolicitud.Approved)
        {
            throw new TransicionEstadoInvalidaException(Estado, EstadoSolicitud.Cancelled);
        }

        if (FechaInicio <= fechaActualDate)
        {
            throw new CancelacionNoPermitidaException("El periodo de vacaciones ya ha iniciado.");
        }

        Estado = EstadoSolicitud.Cancelled;
        AprobadoPor = aprobadorId;
        ActualizadoEn = fechaActual;
    }

    public void Expirar(DateTime fechaActual)
    {
        ValidarTransicion(EstadoSolicitud.Expired);

        Estado = EstadoSolicitud.Expired;
        ActualizadoEn = fechaActual;
    }

    public void Editar(RangoFechas nuevoRango, string nuevoMotivo, DateTime fechaActual)
    {
        if (Estado != EstadoSolicitud.Pending)
        {
            throw new InvalidOperationException("Solo se pueden editar solicitudes en estado Pending.");
        }

        if (string.IsNullOrWhiteSpace(nuevoMotivo))
        {
            throw new ArgumentException("El motivo no puede estar vacío.", nameof(nuevoMotivo));
        }

        FechaInicio = nuevoRango.FechaInicio;
        FechaFin = nuevoRango.FechaFin;
        DiasRequeridos = nuevoRango.CalcularDiasHabiles();
        Motivo = nuevoMotivo.Trim();
        ActualizadoEn = fechaActual;
    }

    public bool EstaEnEstadoFinal()
    {
        return Estado is EstadoSolicitud.Approved 
            or EstadoSolicitud.Rejected 
            or EstadoSolicitud.Cancelled 
            or EstadoSolicitud.Expired;
    }

    public bool PuedeSerCanceladaPorEmpleado()
    {
        return Estado == EstadoSolicitud.Pending;
    }

    public bool PuedeSerEditada()
    {
        return Estado == EstadoSolicitud.Pending;
    }

    private void ValidarTransicion(EstadoSolicitud nuevoEstado)
    {
        var transicionValida = (Estado, nuevoEstado) switch
        {
            (EstadoSolicitud.Pending, EstadoSolicitud.Approved) => true,
            (EstadoSolicitud.Pending, EstadoSolicitud.Rejected) => true,
            (EstadoSolicitud.Pending, EstadoSolicitud.Cancelled) => true,
            (EstadoSolicitud.Pending, EstadoSolicitud.Expired) => true,
            (EstadoSolicitud.Approved, EstadoSolicitud.Cancelled) => true,
            _ => false
        };

        if (!transicionValida)
        {
            throw new TransicionEstadoInvalidaException(Estado, nuevoEstado);
        }
    }

    private void ValidarNoAutoAprobacion(Guid aprobadorId)
    {
        if (aprobadorId == EmpleadoId)
        {
            throw new AutoAprobacionNoPermitidaException();
        }
    }
}
