using System;
using Vacations.Domain.Abstractions;

namespace Vacations.Infrastructure.Time;

/// <summary>
/// Proveedor de tiempo que respeta la zona horaria corporativa.
/// Wrapper sobre TimeProvider para abstraer la zona horaria (RN-27).
/// </summary>
public sealed class ProveedorTiempoSistema : TimeProvider, IProveedorTiempoCorporativo
{
    private readonly TimeProvider _baseProvider;
    private readonly TimeZoneInfo _zonaCorporativa;

    public ProveedorTiempoSistema(TimeProvider baseProvider, string zonaCorporativaId = "Central Standard Time (Mexico)")
    {
        _baseProvider = baseProvider ?? TimeProvider.System;
        try
        {
            _zonaCorporativa = TimeZoneInfo.FindSystemTimeZoneById(zonaCorporativaId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback: intentar ID alternativo para Windows/Linux
            try
            {
                _zonaCorporativa = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                _zonaCorporativa = TimeZoneInfo.Utc;
            }
        }
    }

    public override DateTimeOffset GetUtcNow() => _baseProvider.GetUtcNow();

    public override TimeZoneInfo LocalTimeZone => _zonaCorporativa;

    /// <summary>
    /// Obtiene la fecha actual en la zona horaria corporativa (solo fecha, sin hora).
    /// </summary>
    public DateTime ObtenerFechaActualCorporativa()
    {
        var utcNow = GetUtcNow();
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow.UtcDateTime, _zonaCorporativa);
        return localTime.Date;
    }
}