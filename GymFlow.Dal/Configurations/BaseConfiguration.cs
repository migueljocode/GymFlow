using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GymFlow.Models.Base;

namespace GymFlow.Dal.Configurations;

public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(e => e.UpdatedAt)
            .IsRequired(false);
        
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(e => e.DeletedAt)
            .IsRequired(false);
        
        builder.HasQueryFilter(e => !e.IsDeleted);
        
        builder.HasIndex(e => e.IsDeleted)
            .HasDatabaseName($"IX_{typeof(T).Name}_IsDeleted");
    }
}