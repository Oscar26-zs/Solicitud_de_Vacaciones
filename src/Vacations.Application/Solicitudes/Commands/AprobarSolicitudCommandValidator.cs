using FluentValidation;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Valida la entrada de aprobación de solicitud.</summary>
public sealed class AprobarSolicitudCommandValidator : AbstractValidator<AprobarSolicitudCommand>
{
    public AprobarSolicitudCommandValidator()
    {
        RuleFor(c => c.SolicitudId).NotEmpty();
        RuleFor(c => c.AprobadorEmpleadoId).NotEmpty();
    }
}