using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(b => b.PeriodId).HasColumnName("period_id").IsRequired();
        builder.Property(b => b.CategoryId).HasColumnName("category_id").IsRequired();
        builder.Property(b => b.Amount).HasColumnName("amount").HasColumnType("numeric(12,2)").HasDefaultValue(0m);
        builder.Property(b => b.Percentage).HasColumnName("percentage").HasColumnType("numeric(5,2)");
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

        builder.HasIndex(b => new { b.PeriodId, b.CategoryId }).IsUnique();
        builder.HasIndex(b => b.PeriodId).HasDatabaseName("idx_budgets_period");

        builder.HasOne<Period>().WithMany().HasForeignKey(b => b.PeriodId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Category>().WithMany().HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}
