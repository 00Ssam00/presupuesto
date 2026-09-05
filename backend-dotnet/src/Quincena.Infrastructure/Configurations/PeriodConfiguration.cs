using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Configurations;

public class PeriodConfiguration : IEntityTypeConfiguration<Period>
{
    public void Configure(EntityTypeBuilder<Period> builder)
    {
        builder.ToTable("periods", t => t.HasCheckConstraint("periods_dates_check", "end_date > start_date"));
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Label).HasColumnName("label").IsRequired();
        builder.Property(p => p.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(p => p.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();
        builder.Property(p => p.BaseIncome).HasColumnName("base_income").HasColumnType("numeric(12,2)").HasDefaultValue(0m);
        builder.Property(p => p.Rollover).HasColumnName("rollover").HasColumnType("numeric(12,2)").HasDefaultValue(0m);
        builder.Property(p => p.IsClosed).HasColumnName("is_closed").HasDefaultValue(false);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
    }
}
