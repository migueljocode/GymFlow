namespace GymFlow.Dal.Configurations;

public class WorkoutDayConfiguration : BaseConfiguration<WorkoutDay>
{
    public override void Configure(EntityTypeBuilder<WorkoutDay> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("WorkoutDays");

        builder.Property(wd => wd.DayOfWeek)
            .IsRequired();

        builder.Property(wd => wd.TargetMuscles)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(wd => wd.DurationMinutes)
            .IsRequired();

        builder.Property(wd => wd.Intensity)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(wd => wd.Notes)
            .HasMaxLength(500);

        builder.HasOne(wd => wd.WorkoutPlan)
            .WithMany(wp => wp.WorkoutDays)
            .HasForeignKey(wd => wd.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(wd => wd.WorkoutSessions)
            .WithOne(ws => ws.WorkoutDay)
            .HasForeignKey(ws => ws.WorkoutDayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(wd => wd.WorkoutDayExercises)
            .WithOne(wde => wde.WorkoutDay)
            .HasForeignKey(wde => wde.WorkoutDayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wd => wd.WorkoutPlanId)
            .HasDatabaseName("IX_WorkoutDays_WorkoutPlanId");

        builder.HasIndex(wd => new { wd.WorkoutPlanId, wd.DayOfWeek })
            .IsUnique()
            .HasDatabaseName("IX_WorkoutDays_Plan_Day");
    }
}