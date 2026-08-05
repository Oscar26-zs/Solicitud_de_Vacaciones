using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vacations.Domain.Entities;

namespace Vacations.Infrastructure.Persistence.Configurations;

public sealed class HistorialSolicitudConfiguration : IEntityTypeConfiguration<HistorialSolicitud>
{
    public void Configure(EntityTypeBuilder<HistorialSolicitud> builder)
    {
        builder.ToTable("HistorialSolicitudes");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.SolicitudId).IsRequired();

        builder.Property(h => h.TipoEvento).IsRequired().HasMaxLength(50);

        builder.Property(h => h.EstadoAnterior).HasConversion<int?>().IsRequired(false);
        builder.Property(h => h.EstadoNuevo).HasConversion<int?>().IsRequired(false);

        builder.Property(h => h.CamposModificados).IsRequired(false).HasMaxLength(4000);

        builder.Property(h => h.Actor).IsRequired().HasMaxLength(200);

        builder.Property(h => h.Timestamp).IsRequired();

        builder.Property(h => h.Comentario).IsRequired(false).HasMaxLength(500);

        builder.HasOne<SolicitudVacaciones>()
            .WithMany()
            .HasForeignKey(h => h.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => new { h.SolicitudId, h.Timestamp });
    }
}