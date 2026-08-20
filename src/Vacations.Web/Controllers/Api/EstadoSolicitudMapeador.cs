using Vacations.Domain.Enums;

namespace Vacations.Web.Controllers.Api;

/// <summary>
/// Traduce el EstadoSolicitud interno (5 valores) a los 3 valores exactos que
/// espera el contrato del agente de IA. Cancelled y Expired se agrupan como
/// "rechazada": desde la perspectiva del empleado que consulta el chat, en
/// ambos casos el viaje no va a suceder. Ver TAREAS.md, discrepancia #3.
/// </summary>
public static class EstadoSolicitudMapeador
{
    public static string AEstadoApi(EstadoSolicitud estado) => estado switch
    {
        EstadoSolicitud.Pending => "pendiente",
        EstadoSolicitud.Approved => "aprobada",
        EstadoSolicitud.Rejected => "rechazada",
        EstadoSolicitud.Cancelled => "rechazada",
        EstadoSolicitud.Expired => "rechazada",
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, "Estado de solicitud no soportado por la API del agente de IA.")
    };
}
