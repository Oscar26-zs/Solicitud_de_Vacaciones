using FluentValidation;

namespace Vacations.Application.Solicitudes.Commands;

public sealed class CrearSolicitudCommandValidator : AbstractValidator<CrearSolicitudCommand>
{
    public CrearSolicitudCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.EmpleadoId)
            .NotEmpty()
            .WithMessage("El Id del empleado es requerido.");

        RuleFor(x => x.FechaInicio)
            .NotEmpty()
            .WithMessage("La fecha de inicio es requerida.")
            .Custom((fechaInicio, context) =>
            {
                var ahora = timeProvider.GetUtcNow().DateTime;
                var fechaActual = DateOnly.FromDateTime(ahora);
                var manana = fechaActual.AddDays(1);

                if (fechaInicio < manana)
                {
                    context.AddFailure("FechaInicio", "La fecha de inicio no puede ser anterior a mañana.");
                }
            });

        RuleFor(x => x.FechaFin)
            .NotEmpty()
            .WithMessage("La fecha de fin es requerida.")
            .Custom((fechaFin, context) =>
            {
                if (context.InstanceToValidate.FechaInicio != default && fechaFin < context.InstanceToValidate.FechaInicio)
                {
                    context.AddFailure("FechaFin", "La fecha de fin no puede ser anterior a la de inicio.");
                }
            });

        RuleFor(x => x.FechaInicio)
            .Custom((fechaInicio, context) =>
            {
                var ahora = timeProvider.GetUtcNow().DateTime;
                var fechaActual = DateOnly.FromDateTime(ahora);
                var doseMesesDespues = fechaActual.AddMonths(2);

                if (fechaInicio > doseMesesDespues)
                {
                    context.AddFailure("FechaInicio", "La fecha de inicio no puede exceder 2 meses desde hoy.");
                }
            });

        RuleFor(x => x.Motivo)
            .MaximumLength(1000)
            .WithMessage("El motivo no puede exceder los 1000 caracteres.");
    }
}
