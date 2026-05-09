using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Models.Enums;  // ← این را اضافه کن
using GymFlow.Services.Interfaces;

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
        var logs = await _progressLogRepository.GetWeightTrendAsync(userId, 10);
        var logList = logs.ToList();
        
        if (logList.Count < 3)
            return null;
        
        var weeklyChange = await GetAverageWeeklyChangeAsync(userId, 4);
        if (!weeklyChange.HasValue)
            return null;
        
        var currentWeight = logList.First().Weight;
        var weeksAhead = daysAhead / 7f;
        
        return currentWeight + (weeklyChange.Value * weeksAhead);
    }

    public async Task<PredictionResponse> GetPredictionAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var logs = await _progressLogRepository.GetWeightTrendAsync(userId, 15);
        var logList = logs.ToList();
        
        var currentWeight = logList.FirstOrDefault()?.Weight ?? user?.Weight ?? 0;
        var dataPointsUsed = logList.Count;
        
        var response = new PredictionResponse
        {
            CurrentWeight = currentWeight,
            DataPointsUsed = dataPointsUsed,
            Confidence = dataPointsUsed >= 10 ? "High" : dataPointsUsed >= 5 ? "Medium" : "Low"
        };
        
        if (dataPointsUsed >= 3)
        {
            var weeklyChanges = new List<float>();
            
            for (int i = 0; i < logList.Count - 1; i++)
            {
                var daysDiff = logList[i].LogDate.DayNumber - logList[i + 1].LogDate.DayNumber;
                if (daysDiff > 0)
                {
                    var weeklyChange = (logList[i].Weight - logList[i + 1].Weight) / (daysDiff / 7f);
                    weeklyChanges.Add(weeklyChange);
                }
            }
            
            var avgWeeklyChange = weeklyChanges.Count > 0 ? weeklyChanges.Average() : 0;
            response.AverageWeeklyChange = (float)avgWeeklyChange;
            response.Trend = avgWeeklyChange < -0.1f ? "Losing" : avgWeeklyChange > 0.1f ? "Gaining" : "Maintaining";
            
            response.PredictedWeight7Days = currentWeight + (float)avgWeeklyChange;
            response.PredictedWeight30Days = currentWeight + (float)avgWeeklyChange * 4;
            response.PredictedWeight90Days = currentWeight + (float)avgWeeklyChange * 12;
            
            response.Message = GetRecommendationMessage(avgWeeklyChange, user?.Goal);
        }
        else
        {
            response.Message = $"Need at least 3 weight logs for prediction. Currently have {dataPointsUsed} log(s).";
        }
        
        return response;
    }

    public async Task<float?> GetAverageWeeklyChangeAsync(int userId, int weeks = 4)
    {
        return await _progressLogRepository.GetAverageWeeklyProgressAsync(userId, weeks);
    }

    public async Task<WeightTrendAnalysis> GetWeightTrendAnalysisAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        var logList = logs.ToList();
        
        var analysis = new WeightTrendAnalysis
        {
            DataPointsCount = logList.Count,
            WeightHistory = new List<WeightPointResponse>()
        };
        
        if (!logList.Any())
        {
            analysis.Recommendation = "Start logging your weight to see trends and predictions!";
            return analysis;
        }
        
        var firstLog = logList.Last();
        var lastLog = logList.First();
        
        analysis.CurrentWeight = lastLog.Weight;
        analysis.StartingWeight = firstLog.Weight;
        analysis.TotalChange = lastLog.Weight - firstLog.Weight;
        analysis.TotalChangePercentage = (analysis.TotalChange / firstLog.Weight) * 100;
        analysis.FirstLogDate = firstLog.LogDate;
        analysis.LastLogDate = lastLog.LogDate;
        
        var weeklyChange = await GetAverageWeeklyChangeAsync(userId, 8);
        if (weeklyChange.HasValue)
        {
            analysis.AverageWeeklyChange = weeklyChange.Value;
            analysis.TrendDirection = weeklyChange.Value < -0.1f ? "Losing" : weeklyChange.Value > 0.1f ? "Gaining" : "Maintaining";
            analysis.PredictedWeightNextMonth = lastLog.Weight + (weeklyChange.Value * 4);
            analysis.PredictedWeightNextQuarter = lastLog.Weight + (weeklyChange.Value * 12);
        }
        
        analysis.Recommendation = GenerateRecommendation(analysis, user?.Goal);
        
        foreach (var log in logList.Take(20))
        {
            analysis.WeightHistory.Add(new WeightPointResponse
            {
                Date = log.LogDate,
                Weight = log.Weight,
                BodyFatPercentage = log.BodyFatPercentage
            });
        }
        
        return analysis;
    }

    private string GetRecommendationMessage(double avgWeeklyChange, Goal? goal)
    {
        if (avgWeeklyChange < -0.3f)
            return "Great progress! You're losing weight at a healthy rate. Keep up the consistency! 💪";
        else if (avgWeeklyChange < -0.1f)
            return "You're on the right track! Progress is steady and sustainable. 🎯";
        else if (avgWeeklyChange > 0.3f && goal == Goal.FatLoss)
            return "Your weight is trending upward. Consider reviewing your nutrition and workout intensity. 📈";
        else if (avgWeeklyChange > 0.1f && goal == Goal.MuscleGain)
            return "Good progress! Gaining weight steadily. Make sure it's muscle, not fat. 💪";
        else
            return "You're maintaining well! To see more progress, try increasing workout intensity or adjusting calories. 🔥";
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