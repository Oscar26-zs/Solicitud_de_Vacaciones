namespace Vacations.Domain.Entities;

/// <summary>
/// Representa un empleado del sistema. Los roles (Empleado, Aprobador, RRHH)
/// se gestionan a través de ASP.NET Core Identity, no como campo de esta entidad.
/// </summary>
public sealed class Empleado
{
    public const int EmailMaxLength = 256;
    public const int NombreCompletoMaxLength = 200;

    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string NombreCompleto { get; private set; }

    public DateTime FechaIngreso { get; private set; }

    public bool EstaActivo { get; private set; }

    private Empleado(Guid id, string email, string nombreCompleto, DateTime fechaIngreso, bool estaActivo)
    {
        Id = id;
        Email = email;
        NombreCompleto = nombreCompleto;
        FechaIngreso = fechaIngreso;
        EstaActivo = estaActivo;
    }

    public static Empleado Crear(string email, string nombreCompleto, DateTime fechaIngreso)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El correo electrónico es obligatorio", nameof(email));
        }

        if (email.Length > EmailMaxLength)
        {
            throw new ArgumentException($"El correo electrónico no puede exceder {EmailMaxLength} caracteres", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            throw new ArgumentException("El nombre completo es obligatorio", nameof(nombreCompleto));
        }

        if (nombreCompleto.Length > NombreCompletoMaxLength)
        {
            throw new ArgumentException($"El nombre completo no puede exceder {NombreCompletoMaxLength} caracteres", nameof(nombreCompleto));
        }

        return new Empleado(Guid.NewGuid(), email.Trim(), nombreCompleto.Trim(), fechaIngreso.Date, estaActivo: true);
    }

    public void Activar()
    {
        EstaActivo = true;
    }

    public void Desactivar()
    {
        EstaActivo = false;
    }
}