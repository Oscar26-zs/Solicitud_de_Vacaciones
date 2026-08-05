using FluentValidation;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>Valida la entrada de cancelación de solicitud Approved.</summary>
public sealed class CancelarAprobadaCommandValidator : AbstractValidator<CancelarAprobadaCommand>
{
    public CancelarAprobadaCommandValidator()
    {
        RuleFor(c => c.SolicitudId).NotEmpty();
        RuleFor(c => c.AprobadorEmpleadoId).NotEmpty();
    }
}