using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Configuraciones;

public class ConfiguracionTarjeta : IEntityTypeConfiguration<Tarjeta>
{
    public void Configure(EntityTypeBuilder<Tarjeta> builder)
    {
        builder.ToTable("cards");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Nombre).HasColumnName("name").IsRequired();
        builder.Property(c => c.UltimosCuatro).HasColumnName("last_four").HasColumnType("char(4)");
        builder.Property(c => c.Banco).HasColumnName("bank");
        builder.Property(c => c.EstaActivo).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.CreadoEn).HasColumnName("created_at").HasDefaultValueSql("NOW()");
    }
}
