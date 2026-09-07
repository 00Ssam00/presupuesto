using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Configuraciones;

public class ConfiguracionPagoProximo : IEntityTypeConfiguration<PagoProximo>
{
    public void Configure(EntityTypeBuilder<PagoProximo> builder)
    {
        builder.ToTable("upcoming_payments", t =>
        {
            t.HasCheckConstraint("upcoming_payments_status_check", "status IN ('pending', 'paid', 'overdue')");
            t.HasCheckConstraint("upcoming_payments_recurrence_interval_check", "recurrence_interval IN ('weekly', 'biweekly', 'monthly')");
        });
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(u => u.PeriodoId).HasColumnName("period_id");
        builder.Property(u => u.CategoriaId).HasColumnName("category_id").IsRequired();
        builder.Property(u => u.Nombre).HasColumnName("name").IsRequired();
        builder.Property(u => u.Monto).HasColumnName("amount").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(u => u.FechaVencimiento).HasColumnName("due_date").HasColumnType("date").IsRequired();
        builder.Property(u => u.Estado).HasColumnName("status").HasDefaultValue("pending");
        builder.Property(u => u.EsRecurrente).HasColumnName("is_recurring").HasDefaultValue(false);
        builder.Property(u => u.IntervaloRecurrencia).HasColumnName("recurrence_interval");
        builder.Property(u => u.CreadoEn).HasColumnName("created_at").HasDefaultValueSql("NOW()");

        builder.HasIndex(u => u.FechaVencimiento).HasDatabaseName("idx_upcoming_due");

        builder.HasOne<Periodo>().WithMany().HasForeignKey(u => u.PeriodoId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Categoria>().WithMany().HasForeignKey(u => u.CategoriaId).OnDelete(DeleteBehavior.Restrict);
    }
}
