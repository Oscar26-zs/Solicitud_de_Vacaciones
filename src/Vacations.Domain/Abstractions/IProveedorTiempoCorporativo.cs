namespace Vacations.Domain.Abstractions;

/// <summary>
/// Proveedor de tiempo con soporte para zona horaria corporativa (RN-27).
/// </summary>
public interface IProveedorTiempoCorporativo
{
    /// <summary>
    /// Obtiene la fecha actual en la zona horaria corporativa (solo fecha, sin hora).
    /// </summary>
    DateTime ObtenerFechaActualCorporativa();
}