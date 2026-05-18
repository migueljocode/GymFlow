namespace GymFlow.Models.DTOs.Responses;

public class TodayWorkoutResponse
{
    public DayOfWeek DayOfWeek { get; set; }
    public string TargetMuscles { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Intensity { get; set; } = string.Empty;
    public string DayOfWeekPersian { get; set; } = string.Empty;
}