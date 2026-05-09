using GymFlow.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace GymFlow.Models.DTOs.Requests;

public class CreateWorkoutDayRequest
{
    [Required]
    public int WorkoutPlanId { get; set; }
    
    [Required]
    public DayOfWeek DayOfWeek { get; set; }
    
    public MuscleGroup TargetMuscles { get; set; }
    
    [Range(1, 300)]
    public int DurationMinutes { get; set; }
    
    public Intensity Intensity { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class UpdateWorkoutDayRequest
{
    public DayOfWeek? DayOfWeek { get; set; }
    public MuscleGroup? TargetMuscles { get; set; }
    
    [Range(1, 300)]
    public int? DurationMinutes { get; set; }
    
    public Intensity? Intensity { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}