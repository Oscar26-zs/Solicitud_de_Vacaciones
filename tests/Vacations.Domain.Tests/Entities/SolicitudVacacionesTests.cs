using Vacations.Domain.Entities;
using Vacations.Domain.Enums;
using Vacations.Domain.Exceptions;
using Vacations.Domain.ValueObjects;

namespace Vacations.Domain.Tests.Entities;

public class SolicitudVacacionesTests
{
    private static readonly DateTime Hoy = new(2026, 8, 3);
    private static readonly Guid EmpleadoId = Guid.NewGuid();
    private static readonly Guid AprobadorId = Guid.NewGuid();

    private static SolicitudVacaciones CrearSolicitud(DateTime inicio, DateTime fin, string motivo = "Vacaciones anuales aprobadas por contrato")
        => SolicitudVacaciones.Crear(EmpleadoId, RangoFechas.Crear(inicio, fin, Hoy), motivo, Hoy);

    [Fact]
    public void Crear_DevuelveEstadoInicialPending()
    {
        var solicitud = CrearSolicitud(Hoy.AddDays(5), Hoy.AddDays(7));
        Assert.Equal(EstadoSolicitud.Pending, solicitud.Estado);
        Assert.Equal(EmpleadoId, solicitud.EmpleadoId);
    }

    [Fact]
    public void Aprobar_SolicitudPending_ObtieneApproved()
    {
        var solicitud = CrearSolicitud(Hoy.AddDays(5), Hoy.AddDays(7));
        solicitud.Aprobar(AprobadorId, Hoy);

        Assert.Equal(EstadoSolicitud.Approved, solicitud.Estado);
        Assert.Equal(AprobadorId, solicitud.AprobadoPor);
    }

    [Fact]
    public void Aprobar_PorMismoAutor_LanzaAutoAprobacionNoPermitida()
    {
        var solicitud = CrearSolicitud(Hoy.AddDays(5), Hoy.AddDays(7));

        Assert.Throws<AutoAprobacionNoPermitidaException>(() => solicitud.Aprobar(EmpleadoId, Hoy));
    }

    [Fact]
    public void Rechazar_SinComentario_LanzaArgumentException()
    {
        var solicitud = CrearSolicitud(Hoy.AddDays(5), Hoy.AddDays(7));

        Assert.Throws<ArgumentException>(() => solicitud.Rechazar(AprobadorId, "", Hoy));
    }

    [Fact]
    public void Rechazar_ConComentario_CambiaARejected()
    {
        var solicitud = CrearSolicitud(Hoy.AddDays(5), Hoy.AddDays(7));

        solicitud.Rechazar(AprobadorId, "Motivo del rechazo", Hoy);

        Assert.Equal(EstadoSolicitud.Rejected, solicitud.Estado);
        Assert.Equal("Motivo del rechazo", solicitud.ComentarioAprobador);
    }

    [Fact]
    public void CancelarAprobada_PeriodoYaIniciado_LanzaTransacionInvalida()
    {
        // La solicitud se crea con inicio futuro; al aprobarla, el periodo
        // "ya inició" porque la fecha actual supera a la fecha de inicio.
        var fechaHoyMasLuego = Hoy.AddDays(6);
        var solicitud = CrearSolicitud(Hoy.AddDays(1), Hoy.AddDays(3)); // inicio = mañana
        solicitud.Aprobar(AprobadorId, Hoy);

        // cancelarAprobada evalúa contra fechaActual (fechaHoyMasLuego), que es posterior al inicio.
        Assert.Throws<TransicionEstadoInvalidaException>(() => solicitud.CancelarAprobada(fechaHoyMasLuego));
    }

    [Fact]
    public void CancelarAprobada_FechaInicioFutura_Cancela()
    {
        var solicitud = CrearSolicitud(Hoy.AddDays(5), Hoy.AddDays(7));
        solicitud.Aprobar(AprobadorId, Hoy);

        solicitud.CancelarAprobada(Hoy);

        Assert.Equal(EstadoSolicitud.Cancelled, solicitud.Estado);
    }

    [Fact]
    public void TransicionInvalida_ApprovedARejected_LanzaTransaccionInvalida()
    {
        var solicitud = CrearSolicitud(Hoy.AddDays(5), Hoy.AddDays(7));
        solicitud.Aprobar(AprobadorId, Hoy);

        Assert.Throws<TransicionEstadoInvalidaException>(() => solicitud.Rechazar(AprobadorId, "No", Hoy));
    }
}