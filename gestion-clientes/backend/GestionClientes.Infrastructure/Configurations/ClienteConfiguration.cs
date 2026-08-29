using GestionClientes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionClientes.Infrastructure.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombres)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Apellidos)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Ruc)
            .IsRequired()
            .HasMaxLength(13)
            .IsFixedLength();

        builder.HasIndex(c => c.Ruc)
            .IsUnique();

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Telefono)
            .HasMaxLength(20);

        builder.Property(c => c.Direccion)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(c => c.FechaRegistro)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.Activo)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
