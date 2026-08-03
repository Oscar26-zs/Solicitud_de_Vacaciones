using Vacations.Application.Common;
using Vacations.Application.Solicitudes.Queries;

namespace Vacations.Web.ViewModels;

/// <summary>ViewModel de la bandeja de aprobador (HU-05).</summary>
public sealed class BandejaAprobadorViewModel
{
    public PagedResult<SolicitudBandejaItem> PagedResult { get; set; } = default!;
}