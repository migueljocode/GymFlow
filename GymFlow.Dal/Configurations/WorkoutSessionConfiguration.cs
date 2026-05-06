namespace GymFlow.Dal.Configurations;

public class WorkoutSessionConfiguration : BaseConfiguration<WorkoutSession>
{
    public override void Configure(EntityTypeBuilder<WorkoutSession> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("WorkoutSessions");

        builder.Property(ws => ws.ActualDate)
            .IsRequired();

        builder.Property(ws => ws.ActualDurationMinutes)
            .IsRequired();

        builder.Property(ws => ws.Feeling)
            .HasMaxLength(100);

        builder.HasOne(ws => ws.WorkoutDay)
            .WithMany(wd => wd.WorkoutSessions)
            .HasForeignKey(ws => ws.WorkoutDayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ws => ws.WorkoutDayId)
            .HasDatabaseName("IX_WorkoutSessions_WorkoutDayId");

        builder.HasIndex(ws => ws.ActualDate)
            .HasDatabaseName("IX_WorkoutSessions_ActualDate");

        builder.HasIndex(ws => new { ws.WorkoutDayId, ws.ActualDate })
            .IsUnique()
            .HasDatabaseName("IX_WorkoutSessions_Unique_Attendance");
    }
}