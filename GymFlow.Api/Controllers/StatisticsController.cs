using Microsoft.AspNetCore.Mvc;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Api.Controllers.Base;

namespace GymFlow.Api.Controllers;

/// <summary>
/// Controller for application-wide statistics and analytics
/// </summary>
[Tags("Statistics")]
public class StatisticsController : ApiControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IProgressLogRepository _progressLogRepository;

    public StatisticsController(
        IUserRepository userRepository,
        IWorkoutPlanRepository workoutPlanRepository,
        IWorkoutSessionRepository workoutSessionRepository,
        IExerciseRepository exerciseRepository,
        IProgressLogRepository progressLogRepository)
    {
        _userRepository = userRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _workoutSessionRepository = workoutSessionRepository;
        _exerciseRepository = exerciseRepository;
        _progressLogRepository = progressLogRepository;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStatsAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var userList = users.ToList();
        
        var activePlans = await _workoutPlanRepository.FindAsync(p => p.IsActive);
        var popularExercises = await _exerciseRepository.GetMostUsedExercisesAsync(5);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        
        var sessionsThisWeek = await _workoutSessionRepository.GetSessionsByDateRangeAsync(
            0, startOfWeek, today); // Note: This needs user-specific implementation
        
        var stats = new DashboardStatsResponse
        {
            TotalUsers = userList.Count,
            ActiveUsers = userList.Count(u => u.WorkoutPlans?.Any(p => p.IsActive) == true),
            ActiveWorkoutPlans = activePlans.Count(),
            TotalWorkoutsThisWeek = sessionsThisWeek.Count(),
            MostUsedExercises = popularExercises.Select(e => new PopularExerciseResponse
            {
                Id = e.Id,
                Name = e.Name,
                MuscleGroup = e.PrimaryMuscleGroup.ToString(),
                UsageCount = 0
            }).ToList(),
            UsersByGoal = userList.GroupBy(u => u.Goal.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            UsersByGender = userList.GroupBy(u => u.Gender.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageAge = (float)userList.Average(u => u.Age),
            AverageWeight = userList.Average(u => u.Weight ?? 0),
            NewUsersThisMonth = userList.Count(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1)),
            TotalWorkoutsThisMonth = 0, // Will calculate properly
            AverageWeightLoss = 0, // Will calculate properly
            AverageConsistencyRate = 0, // Will calculate properly
            GeneratedAt = DateTime.UtcNow
        };
        
        return Success<DashboardStatsResponse>(stats);
    }

    /// <summary>
    /// Get user-specific dashboard data
    /// </summary>
    [HttpGet("user/{userId:int}/dashboard")]
    public async Task<IActionResult> GetUserDashboardAsync(int userId)
    {
        var user = await _userRepository.GetUserWithCompleteHistoryAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var activePlan = await _workoutPlanRepository.GetActiveWorkoutPlanAsync(userId);
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        
        var sessionsThisWeek = sessions.Count(s => s.ActualDate >= startOfWeek);
        var sessionsLastWeek = sessions.Count(s => s.ActualDate >= startOfWeek.AddDays(-7) && s.ActualDate < startOfWeek);
        
        var consistencyRate = activePlan?.SessionsPerWeek > 0 
            ? (double)sessionsThisWeek / activePlan.SessionsPerWeek * 100 
            : 0;
        
        // Calculate streak
        var sortedSessions = sessions.OrderByDescending(s => s.ActualDate).ToList();
        var currentStreak = 0;
        var currentDate = today;
        
        while (sortedSessions.Any(s => s.ActualDate == currentDate))
        {
            currentStreak++;
            currentDate = currentDate.AddDays(-1);
        }
        
        var dashboard = new
        {
            user = new
            {
                user.Id,
                user.FullName,
                user.Goal,
                user.Weight,
                user.BodyType,
                memberSince = user.CreatedAt
            },
            activePlan = activePlan != null ? new
            {
                activePlan.Id,
                activePlan.Phase,
                activePlan.SessionsPerWeek,
                activePlan.StartDate,
                activePlan.EndDate
            } : null,
            stats = new
            {
                totalWorkouts = sessions.Count(),
                workoutsThisWeek = sessionsThisWeek,
                workoutsLastWeek = sessionsLastWeek,
                consistencyRate = Math.Min(consistencyRate, 100),
                currentStreak,
                totalWeightLogs = logs.Count(),
                latestWeight = logs.FirstOrDefault()?.Weight,
                startingWeight = logs.LastOrDefault()?.Weight,
                totalWeightChange = logs.FirstOrDefault() != null && logs.LastOrDefault() != null
                    ? logs.First().Weight - logs.Last().Weight
                    : (float?)null
            },
            recentWorkouts = sessions.Take(5).Select(s => new
            {
                s.ActualDate,
                s.WorkoutDay!.DayOfWeek,
                s.ActualDurationMinutes,
                s.Feeling
            }),
            weightHistory = logs.Take(10).Select(l => new
            {
                l.LogDate,
                l.Weight,
                l.BodyFatPercentage
            })
        };
        
        return Success<object>(dashboard);
    }
}