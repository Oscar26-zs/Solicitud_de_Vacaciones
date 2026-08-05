using FluentValidation;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Valida la entrada de cancelación de solicitud Pending.</summary>
public sealed class CancelarSolicitudCommandValidator : AbstractValidator<CancelarSolicitudCommand>
{
    public CancelarSolicitudCommandValidator()
    {
        RuleFor(c => c.SolicitudId).NotEmpty();
        RuleFor(c => c.EmpleadoId).NotEmpty();
    }
}