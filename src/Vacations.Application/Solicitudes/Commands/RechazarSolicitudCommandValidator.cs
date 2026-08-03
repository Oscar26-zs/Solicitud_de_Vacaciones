using FluentValidation;
using Vacations.Domain.Entities;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Valida la entrada de rechazo de solicitud (comentario obligatorio 1..500).</summary>
public sealed class RechazarSolicitudCommandValidator : AbstractValidator<RechazarSolicitudCommand>
{
    public RechazarSolicitudCommandValidator()
    {
        RuleFor(c => c.SolicitudId).NotEmpty();
        RuleFor(c => c.AprobadorEmpleadoId).NotEmpty();

        RuleFor(c => c.Comentario)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(SolicitudVacaciones.ComentarioMaxLength);
    }
}