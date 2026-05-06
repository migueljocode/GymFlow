namespace GymFlow.Models.Entities;

/// <summary>
/// A training plan (e.g., for 2 months) containing multiple workout days.
/// Relationships:
/// - Many-to-one with User
/// - One-to-many with WorkoutDay
/// - One-to-many with ProgressLog (logs linked to this specific plan)
/// </summary>
public class WorkoutPlan : BaseEntity
{
    public int UserId { get; set; }
    public int Phase { get; set; }            // 1,2,3,... increments every ~2 months
    public int SessionsPerWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual ICollection<WorkoutDay> WorkoutDays { get; set; } = new List<WorkoutDay>();
    public virtual ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
}