using Microsoft.AspNetCore.Mvc;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Api.Controllers.Base;
using GymFlow.Api.Helpers;
using GymFlow.Models.Entities;

namespace GymFlow.Api.Controllers;

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

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStatsAsync()
    {
        var users = await _userRepository.GetAllUsersWithPersonAsync();
        var userList = users.ToList();
        
        var activePlans = await _workoutPlanRepository.FindAsync(p => p.IsActive);
        var popularExercises = await _exerciseRepository.GetMostUsedExercisesAsync(5);
        
        var stats = new DashboardStatsResponse
        {
            TotalUsers = userList.Count,
            ActiveUsers = userList.Count(u => u.WorkoutPlans?.Any(p => p.IsActive) == true),
            ActiveWorkoutPlans = activePlans.Count(),
            TotalWorkoutsThisWeek = 0,
            MostUsedExercises = popularExercises.Select(e => new PopularExerciseResponse
            {
                Id = e.Id,
                Name = e.Name,
                MuscleGroup = e.PrimaryMuscleGroup.ToString(),
                UsageCount = 0
            }).ToList(),
            UsersByGoal = userList.GroupBy(u => u.Goal.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            UsersByGender = userList.GroupBy(u => UserHelper.GetGender(u)?.ToString() ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageAge = (float)userList.Average(u => UserHelper.GetAge(u) ?? 0),
            AverageWeight = (float)userList.Average(u => UserHelper.GetWeight(u) ?? 0),
            NewUsersThisMonth = userList.Count(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1)),
            TotalWorkoutsThisMonth = 0,
            AverageWeightLoss = 0,
            AverageConsistencyRate = 0,
            GeneratedAt = DateTime.UtcNow
        };
        
        return Success<DashboardStatsResponse>(stats);
    }

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
        
        var sessionsList = sessions.ToList();
        var sessionsThisWeek = sessionsList.Count(s => s.ActualDate >= startOfWeek);
        var sessionsLastWeek = sessionsList.Count(s => s.ActualDate >= startOfWeek.AddDays(-7) && s.ActualDate < startOfWeek);
        
        var consistencyRate = activePlan?.SessionsPerWeek > 0 
            ? (double)sessionsThisWeek / activePlan.SessionsPerWeek * 100 
            : 0;
        
        var sortedSessions = sessionsList.OrderByDescending(s => s.ActualDate).ToList();
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
                user.Goal,
                fullName = UserHelper.GetFullName(user),
                currentWeight = UserHelper.GetWeight(user),
                bodyType = UserHelper.GetBodyType(user)?.ToString(),
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
                totalWorkouts = sessionsList.Count,
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
            recentWorkouts = sessionsList.Take(5).Select(s => new
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

    [HttpGet("user/{userId:int}/quick-stats")]
    public async Task<IActionResult> GetQuickStatsAsync(int userId)
    {
        var user = await _userRepository.GetUserWithPersonAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var sessionsList = sessions.ToList();
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        var logsList = logs.ToList();
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        
        var stats = new
        {
            TotalWorkouts = sessionsList.Count,
            WorkoutsThisWeek = sessionsList.Count(s => s.ActualDate >= startOfWeek),
            CurrentStreak = await GetCurrentStreakAsync(sessionsList),
            ConsistencyScore = sessionsList.Count > 0 ? 75 : 0,
            CurrentWeight = logsList.FirstOrDefault()?.Weight ?? UserHelper.GetWeight(user) ?? 0,
            TotalWeightChange = (logsList.FirstOrDefault()?.Weight ?? 0) - (logsList.LastOrDefault()?.Weight ?? 0),
            TotalWorkoutMinutes = sessionsList.Sum(s => s.ActualDurationMinutes),
            AchievementsCount = 0
        };
        
        return Success(stats);
    }

    [HttpGet("user/{userId:int}/achievements")]
    public async Task<IActionResult> GetAchievementsAsync(int userId)
    {
        var user = await _userRepository.GetUserWithPersonAsync(userId);
        if (user is null)
            return NotFoundResponse("User", userId);
        
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var sessionsList = sessions.ToList();
        var totalWorkouts = sessionsList.Count;
        var streak = await GetCurrentStreakAsync(sessionsList);
        
        var achievements = new List<object>();
        
        if (totalWorkouts >= 10)
            achievements.Add(new { Name = "Getting Started", Description = "Completed 10 workouts", Icon = "🎯" });
        if (totalWorkouts >= 50)
            achievements.Add(new { Name = "Dedicated Athlete", Description = "Completed 50 workouts", Icon = "🔥" });
        if (streak >= 7)
            achievements.Add(new { Name = "Consistency King", Description = "7-day workout streak", Icon = "👑" });
        if (streak >= 30)
            achievements.Add(new { Name = "Unstoppable", Description = "30-day workout streak", Icon = "⚡" });
        
        if (!achievements.Any())
            achievements.Add(new { Name = "First Step", Description = "Complete your first workout", Icon = "🌟" });
        
        return Success(achievements);
    }

    private async Task<int> GetCurrentStreakAsync(List<WorkoutSession> sessions)
    {
        var sortedSessions = sessions.OrderByDescending(s => s.ActualDate).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentDate = today;
        var streak = 0;
        
        while (sortedSessions.Any(s => s.ActualDate == currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }
        
        return streak;
    }
}