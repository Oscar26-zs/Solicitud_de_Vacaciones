using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;

namespace Vacations.Infrastructure.Persistence.Configurations;

public sealed class SolicitudVacacionesConfiguration : IEntityTypeConfiguration<SolicitudVacaciones>
{
    public void Configure(EntityTypeBuilder<SolicitudVacaciones> builder)
    {
        builder.ToTable("SolicitudesVacaciones");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.EmpleadoId).IsRequired();

        builder.Property(s => s.FechaInicio).IsRequired();
        builder.Property(s => s.FechaFin).IsRequired();
        builder.Property(s => s.DiasRequeridos).IsRequired();

        builder.Property(s => s.Estado)
            .HasConversion<int>()
            .HasDefaultValue(EstadoSolicitud.Pending)
            .IsRequired();

        builder.Property(s => s.Motivo)
            .IsRequired()
            .HasMaxLength(SolicitudVacaciones.MotivoMaxLength);

        builder.Property(s => s.ComentarioAprobador)
            .HasMaxLength(SolicitudVacaciones.ComentarioMaxLength);

        builder.Property(s => s.AprobadoPor).IsRequired(false);

        builder.Property(s => s.CreadoEn).IsRequired();
        builder.Property(s => s.ActualizadoEn).IsRequired();

        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.HasIndex(s => new { s.EmpleadoId, s.Estado });
    }
}