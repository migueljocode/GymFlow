namespace GymFlow.Dal.Configurations;

public class UserConfiguration : BaseConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Users");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Phone)
            .HasMaxLength(15);

        builder.Property(u => u.Email)
            .HasMaxLength(100);

        builder.Property(u => u.Gender)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(u => u.BodyType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Goal)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Weight)
            .HasPrecision(5, 2);

        builder.Property(u => u.Height)
            .HasPrecision(5, 2);

        builder.HasMany(u => u.WorkoutPlans)
            .WithOne(w => w.User)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ProgressLogs)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => new { u.FirstName, u.LastName })
            .HasDatabaseName("IX_Users_FullName");
    }
}