using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vacations.Domain.Entities;
using Vacations.Domain.Enums;

namespace Vacations.Infrastructure.Persistence.Configurations;

public class HistorialSolicitudConfiguration : IEntityTypeConfiguration<HistorialSolicitud>
{
    public void Configure(EntityTypeBuilder<HistorialSolicitud> builder)
    {
        builder.ToTable("HistorialSolicitud");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.SolicitudId)
            .IsRequired();

        builder.Property(h => h.TipoEvento)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(h => h.EstadoAnterior)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(h => h.EstadoNuevo)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(h => h.CamposModificados)
            .HasColumnType("nvarchar(max)");

        builder.Property(h => h.Actor)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(h => h.Timestamp)
            .IsRequired();

        builder.Property(h => h.Comentario)
            .HasMaxLength(500);

        builder.HasIndex(h => h.SolicitudId);
        builder.HasIndex(h => h.Timestamp);

        builder.HasOne<SolicitudVacaciones>()
            .WithMany()
            .HasForeignKey(h => h.SolicitudId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
