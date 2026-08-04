using FluentValidation;

namespace Vacations.Application.Solicitudes.Commands;

public sealed class CrearSolicitudCommandValidator : AbstractValidator<CrearSolicitudCommand>
{
    public CrearSolicitudCommandValidator()
    {
        RuleFor(x => x.EmpleadoId)
            .NotEmpty()
            .WithMessage("El Id del empleado es requerido.");

        RuleFor(x => x.FechaInicio)
            .NotEmpty()
            .WithMessage("La fecha de inicio es requerida.");

        RuleFor(x => x.FechaFin)
            .NotEmpty()
            .WithMessage("La fecha de fin es requerida.");

        RuleFor(x => x.Motivo)
            .NotEmpty()
            .WithMessage("El motivo es requerido.")
            .MinimumLength(10)
            .WithMessage("El motivo debe tener al menos 10 caracteres.")
            .MaximumLength(1000)
            .WithMessage("El motivo no puede exceder los 1000 caracteres.");
    }
}
