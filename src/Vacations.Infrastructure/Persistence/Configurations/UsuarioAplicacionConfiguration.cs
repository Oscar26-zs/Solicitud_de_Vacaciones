using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vacations.Infrastructure.Identity;

namespace Vacations.Infrastructure.Persistence.Configurations;

public sealed class UsuarioAplicacionConfiguration : IEntityTypeConfiguration<UsuarioAplicacion>
{
    public void Configure(EntityTypeBuilder<UsuarioAplicacion> builder)
    {
        builder.ToTable("Usuarios");

        builder.Property(u => u.EmpleadoId).IsRequired(false);

        builder.HasOne(u => u.Empleado)
            .WithMany()
            .HasForeignKey(u => u.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}