namespace GymFlow.Models.Entities;

/// <summary>
/// A specific day inside a WorkoutPlan (e.g., Monday). This is the *planned* template.
/// Relationships:
/// - Many-to-one with WorkoutPlan
/// - One-to-many with WorkoutSession (actual performed sessions)
/// - One-to-many with WorkoutDayExercise (exercises with sets/reps)
/// </summary>
public class WorkoutDay : BaseEntity
{
    public int WorkoutPlanId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public MuscleGroup TargetMuscles { get; set; }   // Flags enum
    public int DurationMinutes { get; set; }
    public Intensity Intensity { get; set; }
    public string? Notes { get; set; }

    public virtual WorkoutPlan WorkoutPlan { get; set; } = null!;
    public virtual ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
    public virtual ICollection<WorkoutDayExercise> WorkoutDayExercises { get; set; } = new List<WorkoutDayExercise>();
}