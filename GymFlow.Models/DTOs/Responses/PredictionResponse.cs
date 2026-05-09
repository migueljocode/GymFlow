namespace GymFlow.Models.DTOs.Responses;

public class PredictionResponse
{
    public float CurrentWeight { get; set; }
    public float? PredictedWeight7Days { get; set; }
    public float? PredictedWeight30Days { get; set; }
    public float? PredictedWeight90Days { get; set; }
    public float AverageWeeklyChange { get; set; }
    public string Trend { get; set; } = string.Empty; // "Losing", "Gaining", "Maintaining"
    public int DataPointsUsed { get; set; }
    public string Confidence { get; set; } = string.Empty; // "High", "Medium", "Low"
    public string Message { get; set; } = string.Empty;
}