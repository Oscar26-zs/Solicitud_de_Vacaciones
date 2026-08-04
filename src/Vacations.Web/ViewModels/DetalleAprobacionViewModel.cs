using Vacations.Application.Solicitudes.Queries;
using Vacations.Domain.Enums;

namespace Vacations.Web.ViewModels;

public class DetalleAprobacionViewModel
{
    public Guid Id { get; set; }
    public Guid EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public string EmpleadoEmail { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int DiasRequeridos { get; set; }
    public EstadoSolicitud Estado { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? ComentarioAprobador { get; set; }
    public DateTime CreadoEn { get; set; }
    public int SaldoDisponible { get; set; }
    public bool TraslapeAprobada { get; set; }
    public bool TraslapePendiente { get; set; }
    public IReadOnlyList<HistorialEventoDto> Historial { get; set; } = [];

    public int PostAprobacion => SaldoDisponible - DiasRequeridos;

    public static DetalleAprobacionViewModel FromDto(DetalleAprobacionDto dto) => new()
    {
        Id = dto.Id,
        EmpleadoId = dto.EmpleadoId,
        EmpleadoNombre = dto.EmpleadoNombre,
        EmpleadoEmail = dto.EmpleadoEmail,
        FechaInicio = dto.FechaInicio,
        FechaFin = dto.FechaFin,
        DiasRequeridos = dto.DiasRequeridos,
        Estado = dto.Estado,
        Motivo = dto.Motivo,
        ComentarioAprobador = dto.ComentarioAprobador,
        CreadoEn = dto.CreadoEn,
        SaldoDisponible = dto.SaldoDisponible,
        TraslapeAprobada = dto.TraslapeAprobada,
        TraslapePendiente = dto.TraslapePendiente,
        Historial = dto.Historial
    };
}
