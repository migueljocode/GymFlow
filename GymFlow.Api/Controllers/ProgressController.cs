using Microsoft.AspNetCore.Mvc;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Models.Entities;
using GymFlow.Api.Controllers.Base;

namespace GymFlow.Api.Controllers;

/// <summary>
/// Controller for managing user progress and weight tracking
/// </summary>
[Tags("Progress")]
public class ProgressController : ApiControllerBase
{
    private readonly IProgressLogRepository _progressLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkoutSessionRepository _workoutSessionRepository;

    public ProgressController(
        IProgressLogRepository progressLogRepository,
        IUserRepository userRepository,
        IWorkoutSessionRepository workoutSessionRepository)
    {
        _progressLogRepository = progressLogRepository;
        _userRepository = userRepository;
        _workoutSessionRepository = workoutSessionRepository;
    }

    /// <summary>
    /// Get all progress logs for a user
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        
        var responses = logs.Select(l => new ProgressLogResponse
        {
            Id = l.Id,
            UserId = l.UserId,
            WorkoutPlanId = l.WorkoutPlanId,
            LogDate = l.LogDate,
            Weight = l.Weight,
            BodyFatPercentage = l.BodyFatPercentage,
            Notes = l.Notes,
            CreatedAt = l.CreatedAt
        }).ToList();
        
        return Success<IEnumerable<ProgressLogResponse>>(responses);
    }

    /// <summary>
    /// Get progress summary with predictions for a user
    /// </summary>
    [HttpGet("user/{userId:int}/summary")]
    public async Task<IActionResult> GetSummaryAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
        
        var logsList = logs.ToList();
        var currentWeight = logsList.FirstOrDefault()?.Weight ?? user.Weight ?? 0;
        var lastWeekLog = logsList.FirstOrDefault(l => l.LogDate >= thisWeekStart.AddDays(-7) && l.LogDate < thisWeekStart);
        var lastMonthLog = logsList.FirstOrDefault(l => l.LogDate >= today.AddDays(-30));
        var firstLog = logsList.LastOrDefault();
        
        var sessionsList = sessions.ToList();
        var sessionsThisWeek = sessionsList.Count(s => s.ActualDate >= thisWeekStart);
        var sessionsLastWeek = sessionsList.Count(s => s.ActualDate >= thisWeekStart.AddDays(-7) && s.ActualDate < thisWeekStart);
        
        // Calculate average weekly change (last 4 weeks)
        var weeklyChanges = new List<float>();
        var sortedLogs = logsList.OrderByDescending(l => l.LogDate).ToList();
        
        for (int i = 0; i < sortedLogs.Count - 1 && i < 8; i++)
        {
            var daysDiff = sortedLogs[i].LogDate.DayNumber - sortedLogs[i + 1].LogDate.DayNumber;
            if (daysDiff > 0)
            {
                var weeklyChange = (sortedLogs[i].Weight - sortedLogs[i + 1].Weight) / (daysDiff / 7f);
                weeklyChanges.Add(weeklyChange);
            }
        }
        
        var avgWeeklyChange = weeklyChanges.Count > 0 ? weeklyChanges.Average() : 0;
        var predictedNextWeek = avgWeeklyChange != 0 ? currentWeight + (float)avgWeeklyChange : (float?)null;
        var predictedNextMonth = avgWeeklyChange != 0 ? currentWeight + (float)avgWeeklyChange * 4 : (float?)null;
        
        var weightHistory = logsList.Take(10).Select(l => new WeightPointResponse
        {
            Date = l.LogDate,
            Weight = l.Weight,
            BodyFatPercentage = l.BodyFatPercentage
        }).ToList();
        
        var response = new ProgressSummaryResponse
        {
            CurrentWeight = currentWeight,
            BodyFatPercentage = logsList.FirstOrDefault()?.BodyFatPercentage,
            WeightChangeLastWeek = lastWeekLog != null ? currentWeight - lastWeekLog.Weight : 0,
            WeightChangeLastMonth = lastMonthLog != null ? currentWeight - lastMonthLog.Weight : 0,
            WeightChangeTotal = firstLog != null ? currentWeight - firstLog.Weight : 0,
            TotalWorkoutSessions = sessionsList.Count,
            WorkoutSessionsThisWeek = sessionsThisWeek,
            WorkoutSessionsLastWeek = sessionsLastWeek,
            AverageSessionDuration = sessionsList.Any() ? sessionsList.Average(s => s.ActualDurationMinutes) : 0,
            PredictedWeightNextWeek = predictedNextWeek,
            PredictedWeightNextMonth = predictedNextMonth,
            Achievement = sessionsThisWeek >= 3 ? "Excellent consistency this week! 🔥" : "Keep pushing! 💪",
            WeightHistory = weightHistory
        };
        
        return Success<ProgressSummaryResponse>(response);
    }

    /// <summary>
    /// Add a new progress log entry
    /// </summary>
    [HttpPost("user/{userId:int}")]
    public async Task<IActionResult> AddProgressLogAsync(int userId, [FromBody] CreateProgressLogRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        // Check if log already exists for this date
        var existingLog = await _progressLogRepository.GetProgressLogByDateAsync(userId, request.LogDate);
        if (existingLog is not null)
            return Error($"Progress log already exists for {request.LogDate}", 409);
        
        var log = new ProgressLog
        {
            UserId = userId,
            LogDate = request.LogDate,
            Weight = request.Weight,
            BodyFatPercentage = request.BodyFatPercentage,
            Notes = request.Notes
        };
        
        var created = await _progressLogRepository.AddAsync(log);
        
        // Update user's current weight
        user.Weight = request.Weight;
        await _userRepository.UpdateAsync(user);
        
        var response = new ProgressLogResponse
        {
            Id = created.Id,
            UserId = created.UserId,
            WorkoutPlanId = created.WorkoutPlanId,
            LogDate = created.LogDate,
            Weight = created.Weight,
            BodyFatPercentage = created.BodyFatPercentage,
            Notes = created.Notes,
            CreatedAt = created.CreatedAt
        };
        
        return CreatedResponse<ProgressLogResponse>(response, "Progress log added successfully");
    }

    /// <summary>
    /// Update a progress log entry
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProgressLogAsync(int id, [FromBody] UpdateProgressLogRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var log = await _progressLogRepository.GetByIdAsync(id);
        if (log is null)
            return NotFoundResponse("ProgressLog", id);
        
        if (request.Weight.HasValue) log.Weight = request.Weight.Value;
        if (request.BodyFatPercentage.HasValue) log.BodyFatPercentage = request.BodyFatPercentage.Value;
        if (request.Notes is not null) log.Notes = request.Notes;
        
        var updated = await _progressLogRepository.UpdateAsync(log);
        
        // Update user's current weight if this is the latest log
        var latestLog = await _progressLogRepository.GetLatestProgressLogAsync(log.UserId);
        if (latestLog?.Id == updated.Id)
        {
            var user = await _userRepository.GetByIdAsync(log.UserId);
            if (user is not null)
            {
                user.Weight = updated.Weight;
                await _userRepository.UpdateAsync(user);
            }
        }
        
        var response = new ProgressLogResponse
        {
            Id = updated.Id,
            UserId = updated.UserId,
            WorkoutPlanId = updated.WorkoutPlanId,
            LogDate = updated.LogDate,
            Weight = updated.Weight,
            BodyFatPercentage = updated.BodyFatPercentage,
            Notes = updated.Notes,
            CreatedAt = updated.CreatedAt
        };
        
        return Success<ProgressLogResponse>(response, "Progress log updated successfully");
    }

    /// <summary>
    /// Delete a progress log entry (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProgressLogAsync(int id)
    {
        var log = await _progressLogRepository.GetByIdAsync(id);
        if (log is null)
            return NotFoundResponse("ProgressLog", id);
        
        await _progressLogRepository.SoftDeleteAsync(id);
        return Success<string?>(null, "Progress log deleted successfully");
    }

    /// <summary>
/// Get weight history for chart
/// </summary>
[HttpGet("user/{userId:int}/weight-history")]
public async Task<IActionResult> GetWeightHistoryAsync(int userId)
{
    var user = await _userRepository.GetByIdAsync(userId);
    if (user is null)
        return NotFoundResponse("User", userId);
    
    var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
    var logList = logs.ToList();
    
    var history = logList.Select(l => new
    {
        Date = l.LogDate,
        Weight = l.Weight,
        BodyFatPercentage = l.BodyFatPercentage
    }).OrderBy(h => h.Date).ToList();
    
    return Success(history);
}
}