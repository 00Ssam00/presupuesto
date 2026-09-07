using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Configuraciones;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals", t => t.HasCheckConstraint("goals_type_check", "type IN ('savings', 'investment')"));
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(g => g.Nombre).HasColumnName("name").IsRequired();
        builder.Property(g => g.Tipo).HasColumnName("type").IsRequired();
        builder.Property(g => g.TargetAmount).HasColumnName("target_amount").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(g => g.CurrentAmount).HasColumnName("current_amount").HasColumnType("numeric(12,2)").HasDefaultValue(0m);
        builder.Property(g => g.Deadline).HasColumnName("deadline").HasColumnType("date");
        builder.Property(g => g.EstaActivo).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(g => g.CreadoEn).HasColumnName("created_at").HasDefaultValueSql("NOW()");
        builder.Property(g => g.ActualizadoEn).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        // Sin relaciones a otras tablas — decisión deliberada, ver plan de migración.
    }
}
