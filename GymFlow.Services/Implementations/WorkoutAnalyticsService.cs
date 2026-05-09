using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Services.Interfaces;
using GymFlow.Services.Models;

namespace GymFlow.Services.Implementations;

public class WorkoutAnalyticsService : IWorkoutAnalyticsService
{
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly IWorkoutDayRepository _workoutDayRepository;
    private readonly IWorkoutPlanRepository _workoutPlanRepository;

    public WorkoutAnalyticsService(
        IWorkoutSessionRepository workoutSessionRepository,
        IWorkoutDayRepository workoutDayRepository,
        IWorkoutPlanRepository workoutPlanRepository)
    {
        _workoutSessionRepository = workoutSessionRepository;
        _workoutDayRepository = workoutDayRepository;
        _workoutPlanRepository = workoutPlanRepository;
    }

    public async Task<int> GetConsistencyScoreAsync(int userId, int weeks = 4)
    {
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var sessionsList = sessions.ToList();
        
        if (!sessionsList.Any())
            return 0;
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weeksAgo = today.AddDays(-weeks * 7);
        
        var recentSessions = sessionsList.Count(s => s.ActualDate >= weeksAgo);
        var expectedSessions = weeks * 3;
        
        return Math.Min(100, (int)((double)recentSessions / expectedSessions * 100));
    }

    public async Task<int> GetCurrentStreakAsync(int userId)
    {
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
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

    public async Task<int> GetLongestStreakAsync(int userId)
    {
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var sortedSessions = sessions.OrderBy(s => s.ActualDate).ToList();
        
        var longestStreak = 0;
        var currentStreak = 0;
        var lastDate = DateOnly.MinValue;
        
        foreach (var session in sortedSessions)
        {
            if (lastDate == DateOnly.MinValue)
            {
                currentStreak = 1;
            }
            else if (session.ActualDate == lastDate.AddDays(1))
            {
                currentStreak++;
            }
            else if (session.ActualDate > lastDate.AddDays(1))
            {
                longestStreak = Math.Max(longestStreak, currentStreak);
                currentStreak = 1;
            }
            lastDate = session.ActualDate;
        }
        
        longestStreak = Math.Max(longestStreak, currentStreak);
        return longestStreak;
    }

    public async Task<Dictionary<string, double>> GetCompletionRateByMuscleGroupAsync(int userId, int weeks = 4)
    {
        return new Dictionary<string, double>();
    }

    public async Task<Dictionary<DayOfWeek, int>> GetBestWorkoutDaysAsync(int userId)
    {
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        
        return sessions
            .GroupBy(s => s.ActualDate.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<List<VolumePoint>> GetVolumeTrendAsync(int userId, int weeks = 8)
    {
        return new List<VolumePoint>();
    }

    public async Task<List<Achievement>> GetUserAchievementsAsync(int userId)
    {
        var streak = await GetCurrentStreakAsync(userId);
        var totalWorkouts = (await _workoutSessionRepository.GetSessionsByUserAsync(userId)).Count();
        
        var achievements = new List<Achievement>();
        
        if (totalWorkouts >= 10)
        {
            achievements.Add(new Achievement 
            { 
                Name = "Getting Started", 
                Description = "Completed 10 workouts", 
                EarnedAt = DateTime.UtcNow, 
                Icon = "🎯" 
            });
        }
        
        if (totalWorkouts >= 50)
        {
            achievements.Add(new Achievement 
            { 
                Name = "Dedicated Athlete", 
                Description = "Completed 50 workouts", 
                EarnedAt = DateTime.UtcNow, 
                Icon = "🔥" 
            });
        }
        
        if (streak >= 7)
        {
            achievements.Add(new Achievement 
            { 
                Name = "Consistency King", 
                Description = "7-day workout streak", 
                EarnedAt = DateTime.UtcNow, 
                Icon = "👑" 
            });
        }
        
        if (streak >= 30)
        {
            achievements.Add(new Achievement 
            { 
                Name = "Unstoppable", 
                Description = "30-day workout streak", 
                EarnedAt = DateTime.UtcNow, 
                Icon = "⚡" 
            });
        }
        
        return achievements;
    }
}