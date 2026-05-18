namespace GymFlow.Models.DTOs.Responses;

public class ExerciseItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
}