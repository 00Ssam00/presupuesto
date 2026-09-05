using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions", t => t.HasCheckConstraint("transactions_type_check", "type IN ('income', 'expense')"));
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.PeriodId).HasColumnName("period_id").IsRequired();
        builder.Property(t => t.CategoryId).HasColumnName("category_id").IsRequired();
        builder.Property(t => t.PaymentMethodId).HasColumnName("payment_method_id").IsRequired();
        builder.Property(t => t.CardId).HasColumnName("card_id");
        builder.Property(t => t.Type).HasColumnName("type").IsRequired();
        builder.Property(t => t.Amount).HasColumnName("amount").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").IsRequired();
        builder.Property(t => t.Note).HasColumnName("note");
        builder.Property(t => t.Date).HasColumnName("date").HasColumnType("date").HasDefaultValueSql("CURRENT_DATE");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

        builder.HasIndex(t => t.PeriodId).HasDatabaseName("idx_transactions_period");
        builder.HasIndex(t => t.CategoryId).HasDatabaseName("idx_transactions_category");
        builder.HasIndex(t => t.Date).HasDatabaseName("idx_transactions_date").IsDescending();

        builder.HasOne<Period>().WithMany().HasForeignKey(t => t.PeriodId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Category>().WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PaymentMethod>().WithMany().HasForeignKey(t => t.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Card>().WithMany().HasForeignKey(t => t.CardId).OnDelete(DeleteBehavior.Restrict);
    }
}
