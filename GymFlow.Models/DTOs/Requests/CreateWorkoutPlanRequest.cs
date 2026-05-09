namespace GymFlow.Models.DTOs.Requests;

public class CreateWorkoutPlanRequest
{
    [Required]
    public int UserId { get; set; }
    
    [Range(1, 10)]
    public int Phase { get; set; }
    
    [Range(1, 7)]
    public int SessionsPerWeek { get; set; }
    
    [Required]
    public DateOnly StartDate { get; set; }
    
    public DateOnly? EndDate { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}