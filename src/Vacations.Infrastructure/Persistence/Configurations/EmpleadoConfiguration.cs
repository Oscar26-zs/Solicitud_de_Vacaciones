using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vacations.Domain.Entities;

namespace Vacations.Infrastructure.Persistence.Configurations;

public sealed class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("Empleados");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(Empleado.EmailMaxLength);

        builder.Property(e => e.NombreCompleto)
            .IsRequired()
            .HasMaxLength(Empleado.NombreCompletoMaxLength);

        builder.Property(e => e.FechaIngreso).IsRequired();

        builder.Property(e => e.EstaActivo).IsRequired();

        builder.HasIndex(e => e.Email).IsUnique();
    }
}