using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vacations.Domain.Entities;

namespace Vacations.Infrastructure.Persistence.Configurations;

public class SaldoEmpleadoConfiguration : IEntityTypeConfiguration<SaldoEmpleado>
{
    public void Configure(EntityTypeBuilder<SaldoEmpleado> builder)
    {
        builder.ToTable("SaldoEmpleado");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.EmpleadoId)
            .IsRequired();

        builder.Property(s => s.SaldoAcumulado)
            .IsRequired();

        builder.Property(s => s.SaldoConsumido)
            .IsRequired();

        builder.Property(s => s.SaldoPendiente)
            .IsRequired();

        builder.Property(s => s.UltimaActualizacion)
            .IsRequired();

        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        builder.Ignore(s => s.SaldoDisponible);

        builder.HasIndex(s => s.EmpleadoId)
            .IsUnique();

        builder.HasOne<Empleado>()
            .WithOne()
            .HasForeignKey<SaldoEmpleado>(s => s.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
