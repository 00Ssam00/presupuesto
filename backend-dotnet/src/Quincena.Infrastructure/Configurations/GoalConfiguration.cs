using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals", t => t.HasCheckConstraint("goals_type_check", "type IN ('savings', 'investment')"));
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(g => g.Name).HasColumnName("name").IsRequired();
        builder.Property(g => g.Type).HasColumnName("type").IsRequired();
        builder.Property(g => g.TargetAmount).HasColumnName("target_amount").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(g => g.CurrentAmount).HasColumnName("current_amount").HasColumnType("numeric(12,2)").HasDefaultValue(0m);
        builder.Property(g => g.Deadline).HasColumnName("deadline").HasColumnType("date");
        builder.Property(g => g.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(g => g.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Property(g => g.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        // Sin relaciones a otras tablas — decisión deliberada, ver plan de migración.
    }
}
