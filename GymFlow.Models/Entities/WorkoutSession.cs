namespace GymFlow.Models.Entities;

/// <summary>
/// An actual performed workout session logged by the user on a specific date.
/// Contains real execution data (duration, feeling).
/// Relationship: Many-to-one with WorkoutDay.
/// </summary>
public class WorkoutSession : BaseEntity
{
    public int WorkoutDayId { get; set; }
    public DateOnly ActualDate { get; set; }
    public int ActualDurationMinutes { get; set; }
    public string? Feeling { get; set; }   // e.g., "tired", "energetic", "knee pain"

    public virtual WorkoutDay WorkoutDay { get; set; } = null!;
}