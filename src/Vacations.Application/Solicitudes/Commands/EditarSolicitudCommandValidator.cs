using FluentValidation;
using Vacations.Domain.Entities;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Valida la entrada de edición de solicitud (solo entrada; negocio en Domain).</summary>
public sealed class EditarSolicitudCommandValidator : AbstractValidator<EditarSolicitudCommand>
{
    public EditarSolicitudCommandValidator()
    {
        RuleFor(c => c.SolicitudId).NotEmpty();

        RuleFor(c => c.EmpleadoId).NotEmpty();

        RuleFor(c => c.FechaInicio).NotEmpty();

        RuleFor(c => c.FechaFin).NotEmpty();

        RuleFor(c => c.Motivo)
            .NotEmpty()
            .MinimumLength(SolicitudVacaciones.MotivoMinLength)
            .MaximumLength(SolicitudVacaciones.MotivoMaxLength);
    }
}