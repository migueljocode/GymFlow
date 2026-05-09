namespace GymFlow.Models.DTOs.Responses;

public class ProgressSummaryResponse
{
    public float CurrentWeight { get; set; }
    public float? BodyFatPercentage { get; set; }
    public float WeightChangeLastWeek { get; set; }
    public float WeightChangeLastMonth { get; set; }
    public float WeightChangeTotal { get; set; }
    public int TotalWorkoutSessions { get; set; }
    public int WorkoutSessionsThisWeek { get; set; }
    public int WorkoutSessionsLastWeek { get; set; }
    public double AverageSessionDuration { get; set; }
    public float? PredictedWeightNextWeek { get; set; }
    public float? PredictedWeightNextMonth { get; set; }
    public string Achievement { get; set; } = string.Empty;
    public List<WeightPointResponse> WeightHistory { get; set; } = new();
}