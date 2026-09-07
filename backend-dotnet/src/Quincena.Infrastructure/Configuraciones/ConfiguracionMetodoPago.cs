using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Configuraciones;

public class ConfiguracionMetodoPago : IEntityTypeConfiguration<MetodoPago>
{
    public void Configure(EntityTypeBuilder<MetodoPago> builder)
    {
        builder.ToTable("payment_methods");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Nombre).HasColumnName("name").IsRequired();
        builder.Property(p => p.RequiereTarjeta).HasColumnName("requires_card").HasDefaultValue(false);
        builder.Property(p => p.EstaActivo).HasColumnName("is_active").HasDefaultValue(true);
    }
}
