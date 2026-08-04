using FluentValidation;

namespace Vacations.Application.Solicitudes.Commands;

public sealed class RechazarSolicitudCommandValidator : AbstractValidator<RechazarSolicitudCommand>
{
    public RechazarSolicitudCommandValidator()
    {
        RuleFor(x => x.SolicitudId)
            .NotEmpty()
            .WithMessage("El Id de la solicitud es requerido.");

        RuleFor(x => x.AprobadorId)
            .NotEmpty()
            .WithMessage("El Id del aprobador es requerido.");

        RuleFor(x => x.Comentario)
            .NotEmpty()
            .WithMessage("El comentario es obligatorio al rechazar una solicitud.")
            .MinimumLength(1)
            .WithMessage("El comentario debe tener al menos 1 carácter.")
            .MaximumLength(500)
            .WithMessage("El comentario no puede exceder los 500 caracteres.");
    }
}
