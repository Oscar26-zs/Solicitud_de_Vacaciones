namespace Vacations.Domain.Enums;

/// <summary>Roles disponibles en el sistema de solicitudes de vacaciones.</summary>
public enum RolUsuario
{
    /// <summary>Empleado: gestiona sus propias solicitudes y consulta su saldo.</summary>
    Empleado,

    /// <summary>Aprobador (rol plano): aprueba/rechaza solicitudes de cualquier empleado, salvo las propias.</summary>
    Aprobador,

    /// <summary>RRHH: acceso de solo lectura a historial y saldos de cualquier empleado.</summary>
    RRHH
}