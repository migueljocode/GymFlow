namespace GymFlow.Models.DTOs.Responses;

public class WorkoutExerciseItemResponse
{
    public int Id { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Sets { get; set; }
    public string Reps { get; set; } = string.Empty;
    public int RestSeconds { get; set; }
    public string? Notes { get; set; }
}