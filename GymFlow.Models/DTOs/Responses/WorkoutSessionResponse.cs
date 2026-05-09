namespace GymFlow.Models.DTOs.Responses;

public class WorkoutSessionResponse
{
    public int Id { get; set; }
    public int WorkoutDayId { get; set; }
    public string WorkoutDayName { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public DateOnly ActualDate { get; set; }
    public int ActualDurationMinutes { get; set; }
    public string? Feeling { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WeeklySummaryResponse
{
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public int TotalSessions { get; set; }
    public Dictionary<string, int> SessionsByDay { get; set; } = new();
    public int TotalDurationMinutes { get; set; }
    public double AverageDurationMinutes { get; set; }
    public int CompletedPlanPercentage { get; set; }
}