using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Configuraciones;

public class ConfiguracionCategoria : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Nombre).HasColumnName("name").IsRequired();
        builder.Property(c => c.Color).HasColumnName("color").IsRequired().HasDefaultValue("#01696f");
        builder.Property(c => c.Icono).HasColumnName("icon");
        builder.Property(c => c.EstaActivo).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.Orden).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(c => c.CreadoEn).HasColumnName("created_at").HasDefaultValueSql("NOW()");
    }
}
