namespace GymFlow.Dal.Configurations;

public class UserConfiguration : BaseConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Users");
        
        builder.Property(u => u.Goal)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(u => u.EstimatedCaloriesIntake)
            .IsRequired(false);
        
        builder.Property(u => u.IsCompetitive)
            .IsRequired()
            .HasDefaultValue(false);
        
        // رابطه با Person (یک به یک)
        builder.HasOne(u => u.Person)
            .WithOne(p => p.User)
            .HasForeignKey<User>(u => u.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // رابطه با WorkoutPlan (یک به چند)
        builder.HasMany(u => u.WorkoutPlans)
            .WithOne(w => w.User)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // رابطه با ProgressLog (یک به چند)
        builder.HasMany(u => u.ProgressLogs)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // ایندکس‌ها
        builder.HasIndex(u => u.PersonId)
            .IsUnique()
            .HasDatabaseName("IX_Users_PersonId");
    }
}