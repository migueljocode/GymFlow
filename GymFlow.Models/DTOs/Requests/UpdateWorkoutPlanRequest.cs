namespace GymFlow.Models.DTOs.Requests;

public class UpdateWorkoutPlanRequest
{
    [Range(1, 10)]
    public int? Phase { get; set; }
    
    [Range(1, 7)]
    public int? SessionsPerWeek { get; set; }
    
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool? IsActive { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}