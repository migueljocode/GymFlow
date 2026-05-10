using System.ComponentModel.DataAnnotations;

namespace GymFlow.Models.DTOs.Requests;

public class CreateWorkoutDayExerciseRequest
{
    [Required]
    public int WorkoutDayId { get; set; }
    
    [Required]
    public int ExerciseId { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int Sets { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Reps { get; set; } = string.Empty;
    
    [Required]
    [Range(30, 180)]
    public int RestSeconds { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}