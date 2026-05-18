namespace GymFlow.Models.DTOs.Responses;

public class WorkoutDayDetailResponse
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int TargetMuscles { get; set; }
    public int DurationMinutes { get; set; }
    public int Intensity { get; set; }
    public string? Notes { get; set; }
    public List<ExerciseInDayResponse> Exercises { get; set; } = new();
}