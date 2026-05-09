using Microsoft.AspNetCore.Mvc;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Api.Controllers.Base;

namespace GymFlow.Api.Controllers;

/// <summary>
/// Controller for weight predictions and trends
/// </summary>
[Tags("Predictions")]
public class PredictionsController : ApiControllerBase
{
    private readonly IProgressLogRepository _progressLogRepository;
    private readonly IUserRepository _userRepository;

    public PredictionsController(
        IProgressLogRepository progressLogRepository,
        IUserRepository userRepository)
    {
        _progressLogRepository = progressLogRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Get weight prediction for a user
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetPredictionAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var logs = await _progressLogRepository.GetWeightTrendAsync(userId, 10);
        var logList = logs.ToList();
        
        var currentWeight = logList.FirstOrDefault()?.Weight ?? user.Weight ?? 0;
        var dataPointsUsed = logList.Count;
        
        var response = new PredictionResponse
        {
            CurrentWeight = currentWeight,
            DataPointsUsed = dataPointsUsed,
            Confidence = dataPointsUsed >= 10 ? "High" : dataPointsUsed >= 5 ? "Medium" : "Low"
        };
        
        if (dataPointsUsed >= 3)
        {
            // Calculate average weekly change using linear regression
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
            
            response.Message = avgWeeklyChange < -0.1f 
                ? "You're on track! Keep up the good work 💪" 
                : avgWeeklyChange > 0.1f 
                    ? "Your weight is trending upward. Consider reviewing your nutrition plan 📈" 
                    : "You're maintaining well! Consistent effort pays off 🎯";
        }
        else
        {
            response.Message = $"Need at least 3 weight logs for prediction. Currently have {dataPointsUsed} log(s).";
        }
        
        return Success<PredictionResponse>(response);
    }

    /// <summary>
    /// Get weight trend analysis
    /// </summary>
    [HttpGet("user/{userId:int}/trend")]
    public async Task<IActionResult> GetTrendAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        var logList = logs.ToList();
        
        if (logList.Count < 2)
        {
            return Success<object>(new
            {
                message = "Not enough data for trend analysis",
                dataPoints = logList.Count
            });
        }
        
        // Calculate weekly averages
        var weeklyAverages = new Dictionary<int, float>();
        var weeklyStart = logList.Last().LogDate;
        
        foreach (var log in logList)
        {
            var weekNumber = (log.LogDate.DayNumber - weeklyStart.DayNumber) / 7;
            if (!weeklyAverages.ContainsKey(weekNumber))
                weeklyAverages[weekNumber] = 0;
            weeklyAverages[weekNumber] = (weeklyAverages[weekNumber] + log.Weight) / 2; // Simplified
        }
        
        var trend = new
        {
            currentWeight = logList.First().Weight,
            startingWeight = logList.Last().Weight,
            totalChange = logList.First().Weight - logList.Last().Weight,
            changePercentage = ((logList.First().Weight - logList.Last().Weight) / logList.Last().Weight) * 100,
            weeklyAverages,
            dataPoints = logList.Count,
            timespan = $"{logList.First().LogDate.DayNumber - logList.Last().LogDate.DayNumber} days"
        };
        
        return Success<object>(trend);
    }
}