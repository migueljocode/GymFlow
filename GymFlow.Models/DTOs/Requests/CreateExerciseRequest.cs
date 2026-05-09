namespace GymFlow.Models.DTOs.Requests;

public class CreateExerciseRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public MuscleGroup PrimaryMuscleGroup { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateExerciseRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    public MuscleGroup? PrimaryMuscleGroup { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}