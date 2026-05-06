namespace GymFlow.Dal.Configurations;

public class WorkoutPlanConfiguration : BaseConfiguration<WorkoutPlan>
{
    public override void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("WorkoutPlans");

        builder.Property(wp => wp.Phase)
            .IsRequired();

        builder.Property(wp => wp.SessionsPerWeek)
            .IsRequired();

        builder.Property(wp => wp.StartDate)
            .IsRequired();

        builder.Property(wp => wp.EndDate)
            .IsRequired(false);

        builder.Property(wp => wp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(wp => wp.Notes)
            .HasMaxLength(500);

        builder.HasOne(wp => wp.User)
            .WithMany(u => u.WorkoutPlans)
            .HasForeignKey(wp => wp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(wp => wp.WorkoutDays)
            .WithOne(wd => wd.WorkoutPlan)
            .HasForeignKey(wd => wd.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(wp => wp.ProgressLogs)
            .WithOne(pl => pl.WorkoutPlan)
            .HasForeignKey(pl => pl.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wp => wp.UserId)
            .HasDatabaseName("IX_WorkoutPlans_UserId");

        builder.HasIndex(wp => wp.IsActive)
            .HasDatabaseName("IX_WorkoutPlans_IsActive");

        builder.HasIndex(wp => new { wp.UserId, wp.IsActive })
            .HasDatabaseName("IX_WorkoutPlans_User_Active");
    }
}