using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vacations.Domain.Entities;

namespace Vacations.Infrastructure.Persistence.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("Empleado");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.NombreCompleto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.FechaIngreso)
            .IsRequired();

        builder.Property(e => e.EstaActivo)
            .IsRequired();

        builder.HasIndex(e => e.Email)
            .IsUnique();
    }
}
