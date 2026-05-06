namespace GymFlow.Models.Entities;

/// <summary>
/// Junction table linking WorkoutDay with Exercise, including sets, reps, rest.
/// Represents the planned details of each exercise on a specific workout day.
/// Relationships:
/// - Many-to-one with WorkoutDay
/// - Many-to-one with Exercise
/// </summary>
public class WorkoutDayExercise : BaseEntity
{
    public int WorkoutDayId { get; set; }
    public int ExerciseId { get; set; }
    public int Sets { get; set; }
    public string Reps { get; set; } = string.Empty;      // e.g., "10,10,8" or "10-12"
    public int RestSeconds { get; set; }
    public string? Notes { get; set; }    // e.g., "slow negative, explosive up"

    public virtual WorkoutDay WorkoutDay { get; set; } = null!;
    public virtual Exercise Exercise { get; set; } = null!;
}