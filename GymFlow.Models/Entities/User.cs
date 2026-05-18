namespace GymFlow.Models.Entities;

public class User : BaseEntity
{
    public int PersonId { get; set; }
    public virtual Person Person { get; set; } = null!;
    
    public Goal Goal { get; set; }
    public int? EstimatedCaloriesIntake { get; set; }
    
    // این خط مهم است - مطمئن شو پیش‌فرض false دارد
    public bool IsCompetitive { get; set; } = false;
    
    public int? CoachId { get; set; }
    public virtual Coach? Coach { get; set; }
    
    public virtual ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
    public virtual ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
}