using FluentValidation;
using Vacations.Domain.Entities;

namespace Vacations.Application.Solicitudes.Commands;

/// <summary>
/// Valida solo la entrada (constitution §3.6). Las reglas de negocio (saldo,
/// traslape) pertenecen al Domain y se validan en el handler.
/// </summary>
public sealed class CrearSolicitudCommandValidator : AbstractValidator<CrearSolicitudCommand>
{
    public CrearSolicitudCommandValidator()
    {
        RuleFor(c => c.EmpleadoId).NotEmpty();

        RuleFor(c => c.FechaInicio).NotEmpty();

        RuleFor(c => c.FechaFin).NotEmpty();

        RuleFor(c => c.Motivo)
            .NotEmpty()
            .MinimumLength(SolicitudVacaciones.MotivoMinLength)
            .MaximumLength(SolicitudVacaciones.MotivoMaxLength);
    }
}