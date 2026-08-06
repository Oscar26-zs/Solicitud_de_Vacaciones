using FluentValidation;

namespace Vacations.Application.Solicitudes.Commands;

public sealed class CancelarAprobadaCommandValidator : AbstractValidator<CancelarAprobadaCommand>
{
    public CancelarAprobadaCommandValidator()
    {
        RuleFor(x => x.SolicitudId)
            .NotEmpty()
            .WithMessage("El Id de la solicitud es requerido.");

        RuleFor(x => x.AprobadorId)
            .NotEmpty()
            .WithMessage("El Id del aprobador es requerido.");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .WithMessage("El motivo de cancelación es obligatorio.")
            .MinimumLength(1)
            .WithMessage("El motivo debe tener al menos 1 carácter.")
            .MaximumLength(500)
            .WithMessage("El motivo no puede exceder los 500 caracteres.");
    }
}
