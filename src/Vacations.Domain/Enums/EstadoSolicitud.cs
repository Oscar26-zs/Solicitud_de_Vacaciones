namespace Vacations.Domain.Enums;

/// <summary>Estados posibles de una solicitud de vacaciones.</summary>
public enum EstadoSolicitud
{
    /// <summary>Solicitud creada, esperando decisión de un aprobador.</summary>
    Pending,

    /// <summary>Solicitud aprobada; el saldo ha sido descontado.</summary>
    Approved,

    /// <summary>Solicitud rechazada por un aprobador con comentario obligatorio.</summary>
    Rejected,

    /// <summary>Solicitud cancelada por el empleado (Pending) o por un aprobador (Approved antes del inicio).</summary>
    Cancelled,

    /// <summary>Solicitud pendiente que no fue resuelta antes de alcanzar su fecha de inicio.</summary>
    Expired
}