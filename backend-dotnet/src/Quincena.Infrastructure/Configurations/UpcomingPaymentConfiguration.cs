using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Configurations;

public class UpcomingPaymentConfiguration : IEntityTypeConfiguration<UpcomingPayment>
{
    public void Configure(EntityTypeBuilder<UpcomingPayment> builder)
    {
        builder.ToTable("upcoming_payments", t =>
        {
            t.HasCheckConstraint("upcoming_payments_status_check", "status IN ('pending', 'paid', 'overdue')");
            t.HasCheckConstraint("upcoming_payments_recurrence_interval_check", "recurrence_interval IN ('weekly', 'biweekly', 'monthly')");
        });
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(u => u.PeriodId).HasColumnName("period_id");
        builder.Property(u => u.CategoryId).HasColumnName("category_id").IsRequired();
        builder.Property(u => u.Name).HasColumnName("name").IsRequired();
        builder.Property(u => u.Amount).HasColumnName("amount").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(u => u.DueDate).HasColumnName("due_date").HasColumnType("date").IsRequired();
        builder.Property(u => u.Status).HasColumnName("status").HasDefaultValue("pending");
        builder.Property(u => u.IsRecurring).HasColumnName("is_recurring").HasDefaultValue(false);
        builder.Property(u => u.RecurrenceInterval).HasColumnName("recurrence_interval");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");

        builder.HasIndex(u => u.DueDate).HasDatabaseName("idx_upcoming_due");

        builder.HasOne<Period>().WithMany().HasForeignKey(u => u.PeriodId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Category>().WithMany().HasForeignKey(u => u.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
