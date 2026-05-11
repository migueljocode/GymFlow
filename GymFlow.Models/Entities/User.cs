namespace GymFlow.Models.Entities;

public class User : BaseEntity
{
    public int PersonId { get; set; }
    public virtual Person Person { get; set; } = null!;
    
    // اطلاعات مخصوص کاربر
    public Goal Goal { get; set; }
    public int? EstimatedCaloriesIntake { get; set; }
    public bool IsCompetitive { get; set; }
    
    // روابط
    public virtual ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
    public virtual ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
}