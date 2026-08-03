namespace Vacations.Application.Abstractions;

/// <summary>
/// Provee información del usuario autenticado en ejecución. La implementación
/// concreta vive en la capa Web (HttpContext) y se registra en DI.
/// </summary>
public interface IUsuarioActual
{
    /// <summary>Identificador del usuario; null si la solicitud es anónima.</summary>
    Guid? UsuarioId { get; }

    /// <summary>Id del Empleado asociado; null si el usuario no tiene empleado vinculado.</summary>
    Guid? EmpleadoId { get; }

    /// <summary>Email del usuario conectado.</summary>
    string? Email { get; }

    /// <summary>Roles del usuario.</summary>
    IReadOnlyCollection<string> Roles { get; }
}