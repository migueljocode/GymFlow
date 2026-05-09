namespace GymFlow.Models.DTOs.Requests;

public class LogWorkoutRequest
{
    [Required]
    public int WorkoutDayId { get; set; }
    
    [Required]
    public DateOnly ActualDate { get; set; }
    
    [Range(1, 300)]
    public int ActualDurationMinutes { get; set; }
    
    [MaxLength(500)]
    public string? Feeling { get; set; }
}