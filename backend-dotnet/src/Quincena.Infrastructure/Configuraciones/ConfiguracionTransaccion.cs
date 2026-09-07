using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Configuraciones;

public class ConfiguracionTransaccion : IEntityTypeConfiguration<Transaccion>
{
    public void Configure(EntityTypeBuilder<Transaccion> builder)
    {
        builder.ToTable("transactions", t => t.HasCheckConstraint("transactions_type_check", "type IN ('income', 'expense')"));
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.PeriodoId).HasColumnName("period_id").IsRequired();
        builder.Property(t => t.CategoriaId).HasColumnName("category_id").IsRequired();
        builder.Property(t => t.MetodoPagoId).HasColumnName("payment_method_id").IsRequired();
        builder.Property(t => t.TarjetaId).HasColumnName("card_id");
        builder.Property(t => t.Tipo).HasColumnName("type").IsRequired();
        builder.Property(t => t.Monto).HasColumnName("amount").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(t => t.Descripcion).HasColumnName("description").IsRequired();
        builder.Property(t => t.Nota).HasColumnName("note");
        builder.Property(t => t.Fecha).HasColumnName("date").HasColumnType("date").HasDefaultValueSql("CURRENT_DATE");
        builder.Property(t => t.CreadoEn).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Property(t => t.ActualizadoEn).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

        builder.HasIndex(t => t.PeriodoId).HasDatabaseName("idx_transactions_period");
        builder.HasIndex(t => t.CategoriaId).HasDatabaseName("idx_transactions_category");
        builder.HasIndex(t => t.Fecha).HasDatabaseName("idx_transactions_date").IsDescending();

        builder.HasOne<Periodo>().WithMany().HasForeignKey(t => t.PeriodoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Categoria>().WithMany().HasForeignKey(t => t.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MetodoPago>().WithMany().HasForeignKey(t => t.MetodoPagoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Tarjeta>().WithMany().HasForeignKey(t => t.TarjetaId).OnDelete(DeleteBehavior.Restrict);
    }
}
