namespace GymFlow.Models.Entities;

/// <summary>
/// End user who logs workouts and tracks progress.
/// Relationships:
/// - One-to-many with WorkoutPlan (a user can have multiple plans over time)
/// - One-to-many with ProgressLog (user's weight/BF% history)
/// </summary>
public class User : Person
{
    public Goal Goal { get; set; }
    public int? EstimatedCaloriesIntake { get; set; }
    public bool IsCompetitive { get; set; }

    public virtual ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
    public virtual ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
}
