namespace GymFlow.Dal.Configurations;

public class WorkoutDayExerciseConfiguration : BaseConfiguration<WorkoutDayExercise>
{
    public override void Configure(EntityTypeBuilder<WorkoutDayExercise> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("WorkoutDayExercises");

        builder.Property(wde => wde.Sets)
            .IsRequired();

        builder.Property(wde => wde.Reps)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(wde => wde.RestSeconds)
            .IsRequired();

        builder.Property(wde => wde.Notes)
            .HasMaxLength(500);

        builder.HasOne(wde => wde.WorkoutDay)
            .WithMany(wd => wd.WorkoutDayExercises)
            .HasForeignKey(wde => wde.WorkoutDayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wde => wde.Exercise)
            .WithMany(e => e.WorkoutDayExercises)
            .HasForeignKey(wde => wde.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wde => wde.WorkoutDayId)
            .HasDatabaseName("IX_WorkoutDayExercises_WorkoutDayId");

        builder.HasIndex(wde => wde.ExerciseId)
            .HasDatabaseName("IX_WorkoutDayExercises_ExerciseId");

        builder.HasIndex(wde => new { wde.WorkoutDayId, wde.ExerciseId })
            .IsUnique()
            .HasDatabaseName("IX_WorkoutDayExercises_Unique");
    }
}