namespace GymFlow.Models.Entities;

/// <summary>
/// User's physical progress entry (weight, body fat, notes) logged over time.
/// Relationships:
/// - Many-to-one with User
/// - Optional many-to-one with WorkoutPlan (to know under which plan the progress happened)
/// </summary>
public class ProgressLog : BaseEntity
{
    public int UserId { get; set; }
    public int? WorkoutPlanId { get; set; }
    public DateOnly LogDate { get; set; }
    public float Weight { get; set; }            // kg
    public float? BodyFatPercentage { get; set; }
    public string? Notes { get; set; }

    public virtual User User { get; set; } = null!;
    
    // People take breaks, get injured, or simply skip tracking for a few weeks.
    // you still want their historical data without forcing ProgressLog assigned to WorkingPlan.
    // that way you can see exact Logs that have no WorkoutPlan and their Damage to Progress.
    public virtual WorkoutPlan? WorkoutPlan { get; set; } 
}