namespace GymFlow.Services.Implementations;

public class WeightPredictionService : IWeightPredictionService
{
    private readonly IProgressLogRepository _progressLogRepository;
    private readonly IUserRepository _userRepository;

    public WeightPredictionService(
        IProgressLogRepository progressLogRepository,
        IUserRepository userRepository)
    {
        _progressLogRepository = progressLogRepository;
        _userRepository = userRepository;
    }

    public async Task<float?> PredictWeightAsync(int userId, int daysAhead = 7)
    {
        var logs = await GetWeightLogsAsync(userId, 10);
        if (logs.Count < 3) return null;
        
        var weeklyChange = await GetAverageWeeklyChangeAsync(userId, 4);
        if (!weeklyChange.HasValue) return null;
        
        var weeksAhead = daysAhead / 7f;
        return logs.First().Weight + (weeklyChange.Value * weeksAhead);
    }

    public async Task<PredictionResponse> GetPredictionAsync(int userId)
    {
        var user = await _userRepository.GetUserWithPersonAsync(userId);
        var logs = await GetWeightLogsAsync(userId, 15);
        
        var currentWeight = logs.FirstOrDefault()?.Weight ?? user?.Person?.Weight ?? 0;
        var response = CreateBasePredictionResponse(currentWeight, logs.Count);
        
        if (logs.Count >= 3)
        {
            var avgWeeklyChange = CalculateAverageWeeklyChange(logs);
            response = PopulatePredictionResponse(response, currentWeight, avgWeeklyChange, user?.Person?.User?.Goal);
        }
        
        return response;
    }

    public async Task<float?> GetAverageWeeklyChangeAsync(int userId, int weeks = 4)
    {
        return await _progressLogRepository.GetAverageWeeklyProgressAsync(userId, weeks);
    }

    public async Task<WeightTrendAnalysis> GetWeightTrendAnalysisAsync(int userId)
    {
        var user = await _userRepository.GetUserWithPersonAsync(userId);
        var logs = await GetWeightLogsAsync(userId, 20);
        
        var analysis = new WeightTrendAnalysis { WeightHistory = new List<WeightPointResponse>() };
        
        if (!logs.Any())
        {
            analysis.Recommendation = "Start logging your weight to see trends and predictions!";
            return analysis;
        }
        
        var (firstLog, lastLog) = (logs.Last(), logs.First());
        var weeklyChange = await GetAverageWeeklyChangeAsync(userId, 8);
        
        analysis.CurrentWeight = lastLog.Weight;
        analysis.StartingWeight = firstLog.Weight;
        analysis.TotalChange = lastLog.Weight - firstLog.Weight;
        analysis.TotalChangePercentage = (analysis.TotalChange / firstLog.Weight) * 100;
        analysis.FirstLogDate = firstLog.LogDate;
        analysis.LastLogDate = lastLog.LogDate;
        analysis.DataPointsCount = logs.Count;
        
        if (weeklyChange.HasValue)
        {
            analysis.AverageWeeklyChange = weeklyChange.Value;
            analysis.TrendDirection = GetTrendDirection(weeklyChange.Value);
            analysis.PredictedWeightNextMonth = lastLog.Weight + (weeklyChange.Value * 4);
            analysis.PredictedWeightNextQuarter = lastLog.Weight + (weeklyChange.Value * 12);
        }
        
        analysis.Recommendation = GenerateRecommendation(analysis, user?.Person?.User?.Goal);
        analysis.WeightHistory = logs.Select(l => new WeightPointResponse
        {
            Date = l.LogDate,
            Weight = l.Weight,
            BodyFatPercentage = l.BodyFatPercentage
        }).ToList();
        
        return analysis;
    }

    // ========== Private Helper Methods ==========
    
    private async Task<List<ProgressLog>> GetWeightLogsAsync(int userId, int count)
    {
        var logs = await _progressLogRepository.GetWeightTrendAsync(userId, count);
        return logs.ToList();
    }
    
    private PredictionResponse CreateBasePredictionResponse(float currentWeight, int dataPoints)
    {
        return new PredictionResponse
        {
            CurrentWeight = currentWeight,
            DataPointsUsed = dataPoints,
            Confidence = dataPoints >= 10 ? "High" : dataPoints >= 5 ? "Medium" : "Low"
        };
    }
    
    private float CalculateAverageWeeklyChange(List<ProgressLog> logs)
    {
        // logs are expected in descending order (newest first) from repository
        // For calculation we need chronological order, so we reverse
        var chronological = logs.OrderBy(l => l.LogDate).ToList();
        var weeklyChanges = new List<float>();
        
        for (int i = 0; i < chronological.Count - 1; i++)
        {
            var daysDiff = chronological[i + 1].LogDate.DayNumber - chronological[i].LogDate.DayNumber;
            if (daysDiff > 0)
            {
                var weeklyChange = (chronological[i + 1].Weight - chronological[i].Weight) / (daysDiff / 7f);
                weeklyChanges.Add(weeklyChange);
            }
        }
        
        return weeklyChanges.Count > 0 ? weeklyChanges.Average() : 0;
    }
    
    private PredictionResponse PopulatePredictionResponse(PredictionResponse response, float currentWeight, float avgWeeklyChange, Goal? goal)
    {
        response.AverageWeeklyChange = avgWeeklyChange;
        response.Trend = GetTrendDirection(avgWeeklyChange);
        response.PredictedWeight7Days = currentWeight + avgWeeklyChange;
        response.PredictedWeight30Days = currentWeight + (avgWeeklyChange * 4);
        response.PredictedWeight90Days = currentWeight + (avgWeeklyChange * 12);
        response.Message = GetRecommendationMessage(avgWeeklyChange, goal);
        
        return response;
    }
    
    private string GetTrendDirection(float change)
    {
        return change < -0.1f ? "Losing" : change > 0.1f ? "Gaining" : "Maintaining";
    }
    
    private string GetRecommendationMessage(float avgWeeklyChange, Goal? goal)
    {
        if (avgWeeklyChange < -0.3f)
            return "Great progress! You're losing weight at a healthy rate. Keep up the consistency! ";
        if (avgWeeklyChange < -0.1f)
            return "You're on the right track! Progress is steady and sustainable. ";
        if (avgWeeklyChange > 0.3f && goal == Goal.FatLoss)
            return "Your weight is trending upward. Consider reviewing your nutrition and workout intensity. 📈";
        if (avgWeeklyChange > 0.1f && goal == Goal.MuscleGain)
            return "Good progress! Gaining weight steadily. Make sure it's muscle, not fat. ";
        
        return "You're maintaining well! To see more progress, try increasing workout intensity or adjusting calories. ";
    }
    
    private string GenerateRecommendation(WeightTrendAnalysis analysis, Goal? goal)
    {
        if (analysis.DataPointsCount < 5)
            return "Log your weight weekly for better insights and more accurate predictions.";
        
        if (goal == Goal.FatLoss && analysis.TotalChange > 0)
            return "Your weight has increased since starting. Consider tracking your calorie intake and increasing cardio.";
        
        if (goal == Goal.MuscleGain && analysis.TotalChange < 0)
            return "You're losing weight but aiming to gain muscle. Increase protein intake and focus on progressive overload.";
        
        if (Math.Abs(analysis.AverageWeeklyChange) < 0.05f)
            return "Weight is stable. To reach your goal, try small adjustments to your diet or training plan.";
        
        if (analysis.TrendDirection == "Losing" && goal == Goal.FatLoss)
            return $"You're losing {Math.Abs(analysis.AverageWeeklyChange):F1}kg/week - excellent progress! Stay consistent.";
        
        if (analysis.TrendDirection == "Gaining" && goal == Goal.MuscleGain)
            return $"You're gaining {analysis.AverageWeeklyChange:F1}kg/week - great for muscle building!";
        
        return "Keep tracking your progress and stay consistent with your workouts!";
    }
}