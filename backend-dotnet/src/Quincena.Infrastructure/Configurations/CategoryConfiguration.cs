using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Name).HasColumnName("name").IsRequired();
        builder.Property(c => c.Color).HasColumnName("color").IsRequired().HasDefaultValue("#01696f");
        builder.Property(c => c.Icon).HasColumnName("icon");
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
    }
}
