namespace GymFlow.Services.Interfaces;

/// <summary>
/// Service for predicting user weight based on historical data
/// </summary>
public interface IWeightPredictionService
{
    /// <summary>
    /// Predicts weight after specified number of days
    /// </summary>
    Task<float?> PredictWeightAsync(int userId, int daysAhead = 7);
    
    /// <summary>
    /// Gets complete prediction response with trend analysis
    /// </summary>
    Task<PredictionResponse> GetPredictionAsync(int userId);
    
    /// <summary>
    /// Calculates average weekly weight change
    /// </summary>
    Task<float?> GetAverageWeeklyChangeAsync(int userId, int weeks = 4);
    
    /// <summary>
    /// Gets weight trend analysis with recommendations
    /// </summary>
    Task<WeightTrendAnalysis> GetWeightTrendAnalysisAsync(int userId);
}

/// <summary>
/// Detailed weight trend analysis result
/// </summary>
public class WeightTrendAnalysis
{
    public float CurrentWeight { get; set; }
    public float StartingWeight { get; set; }
    public float TotalChange { get; set; }
    public float TotalChangePercentage { get; set; }
    public float AverageWeeklyChange { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public float? PredictedWeightNextMonth { get; set; }
    public float? PredictedWeightNextQuarter { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public List<WeightPointResponse> WeightHistory { get; set; } = new();
    public int DataPointsCount { get; set; }
    public DateOnly FirstLogDate { get; set; }
    public DateOnly LastLogDate { get; set; }
}