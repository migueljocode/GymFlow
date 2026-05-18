namespace GymFlow.Models.DTOs.Requests;

public class UpdateWorkoutDayExerciseRequest
{
    public int? Sets { get; set; }
    public string? Reps { get; set; }
    public int? RestSeconds { get; set; }
    public string? Notes { get; set; }
}