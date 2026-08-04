using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;

namespace Vacations.Infrastructure.Persistence.Configurations;

public class SolicitudVacacionesConfiguration : IEntityTypeConfiguration<SolicitudVacaciones>
{
    public void Configure(EntityTypeBuilder<SolicitudVacaciones> builder)
    {
        builder.ToTable("SolicitudVacaciones");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.EmpleadoId)
            .IsRequired();

        builder.Property(s => s.FechaInicio)
            .IsRequired();

        builder.Property(s => s.FechaFin)
            .IsRequired();

        builder.Property(s => s.DiasRequeridos)
            .IsRequired();

        builder.Property(s => s.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Motivo)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.ComentarioAprobador)
            .HasMaxLength(500);

        builder.Property(s => s.CreadoEn)
            .IsRequired();

        builder.Property(s => s.ActualizadoEn)
            .IsRequired();

        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        builder.HasIndex(s => s.EmpleadoId);
        builder.HasIndex(s => s.Estado);
        builder.HasIndex(s => s.FechaInicio);

        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(s => s.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
