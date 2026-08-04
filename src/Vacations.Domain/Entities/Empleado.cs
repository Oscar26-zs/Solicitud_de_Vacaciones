namespace Vacations.Domain.Entities;

public sealed class Empleado
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string NombreCompleto { get; private set; } = string.Empty;
    public DateOnly FechaIngreso { get; private set; }
    public bool EstaActivo { get; private set; }

    private Empleado()
    {
    }

    public static Empleado Crear(string email, string nombreCompleto, DateOnly fechaIngreso)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El email no puede estar vacío.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            throw new ArgumentException("El nombre completo no puede estar vacío.", nameof(nombreCompleto));
        }

        return new Empleado
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            NombreCompleto = nombreCompleto.Trim(),
            FechaIngreso = fechaIngreso,
            EstaActivo = true
        };
    }

    public void Desactivar()
    {
        EstaActivo = false;
    }

    public void Activar()
    {
        EstaActivo = true;
    }

    public void ActualizarNombre(string nombreCompleto)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            throw new ArgumentException("El nombre completo no puede estar vacío.", nameof(nombreCompleto));
        }

        NombreCompleto = nombreCompleto.Trim();
    }
}
