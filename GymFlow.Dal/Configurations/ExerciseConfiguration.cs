namespace GymFlow.Dal.Configurations;

public class ExerciseConfiguration : BaseConfiguration<Exercise>
{
    public override void Configure(EntityTypeBuilder<Exercise> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Exercises");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.PrimaryMuscleGroup)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.HasMany(e => e.WorkoutDayExercises)
            .WithOne(wde => wde.Exercise)
            .HasForeignKey(wde => wde.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("IX_Exercises_Name");
    }
}