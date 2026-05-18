namespace GymFlow.Services.Implementations;

public class UserDashboardService : IUserDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly IProgressLogRepository _progressLogRepository;
    private readonly IWorkoutAnalyticsService _analyticsService;
    private readonly IWeightPredictionService _predictionService;

    public UserDashboardService(
        IUserRepository userRepository,
        IWorkoutPlanRepository workoutPlanRepository,
        IWorkoutSessionRepository workoutSessionRepository,
        IProgressLogRepository progressLogRepository,
        IWorkoutAnalyticsService analyticsService,
        IWeightPredictionService predictionService)
    {
        _userRepository = userRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _workoutSessionRepository = workoutSessionRepository;
        _progressLogRepository = progressLogRepository;
        _analyticsService = analyticsService;
        _predictionService = predictionService;
    }

    public async Task<UserDashboardData> GetUserDashboardAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new Exception($"User with ID {userId} not found");
        
        var stats = await GetQuickStatsAsync(userId);
        var activities = await GetRecentActivitiesAsync(userId, 10);
        var achievements = await _analyticsService.GetUserAchievementsAsync(userId);
        var activePlan = await _workoutPlanRepository.GetActiveWorkoutPlanAsync(userId);
        var prediction = await _predictionService.GetPredictionAsync(userId);
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);
        var sessions = await _workoutSessionRepository.GetSessionsByDateRangeAsync(userId, startOfWeek, endOfWeek);
        
        var weeklySummary = new WeeklySummaryResponse
        {
            WeekStart = startOfWeek,
            WeekEnd = endOfWeek,
            TotalSessions = sessions.Count(),
            TotalDurationMinutes = sessions.Sum(s => s.ActualDurationMinutes),
            AverageDurationMinutes = sessions.Any() ? sessions.Average(s => s.ActualDurationMinutes) : 0,
            CompletedPlanPercentage = activePlan?.SessionsPerWeek > 0 
                ? (int)((double)sessions.Count() / activePlan.SessionsPerWeek * 100) 
                : 0,
            SessionsByDay = sessions.GroupBy(s => s.ActualDate.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };
        
        var logs = await _progressLogRepository.GetWeightTrendAsync(userId, 10);
        var weightHistory = logs.Select(l => new WeightPointResponse
        {
            Date = l.LogDate,
            Weight = l.Weight,
            BodyFatPercentage = l.BodyFatPercentage
        }).ToList();
        
        WorkoutPlanResponse? planResponse = null;
        if (activePlan is not null)
        {
            planResponse = new WorkoutPlanResponse
            {
                Id = activePlan.Id,
                UserId = activePlan.UserId,
                Phase = activePlan.Phase,
                SessionsPerWeek = activePlan.SessionsPerWeek,
                StartDate = activePlan.StartDate,
                EndDate = activePlan.EndDate,
                IsActive = activePlan.IsActive,
                Notes = activePlan.Notes,
                CreatedAt = activePlan.CreatedAt,
                CompletedSessionsCount = 0,
                TotalSessionsCount = activePlan.SessionsPerWeek * 8,
                AverageWeightDuringPlan = null
            };
        }
        
        return new UserDashboardData
        {
            Stats = stats,
            RecentActivities = activities,
            Achievements = achievements,
            ActivePlan = planResponse,
            WeightPrediction = prediction,
            WeeklySummary = weeklySummary,
            WeightHistory = weightHistory
        };
    }

    public async Task<QuickStats> GetQuickStatsAsync(int userId)
    {
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var sessionsList = sessions.ToList();
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        var logsList = logs.ToList();
        
        var currentWeight = logsList.FirstOrDefault()?.Weight ?? 0;
        var firstWeight = logsList.LastOrDefault()?.Weight ?? currentWeight;
        
        return new QuickStats
        {
            TotalWorkouts = sessionsList.Count,
            WorkoutsThisWeek = sessionsList.Count(s => s.ActualDate >= startOfWeek),
            CurrentStreak = await _analyticsService.GetCurrentStreakAsync(userId),
            ConsistencyScore = await _analyticsService.GetConsistencyScoreAsync(userId),
            CurrentWeight = currentWeight,
            TotalWeightChange = currentWeight - firstWeight,
            TotalWorkoutMinutes = sessionsList.Sum(s => s.ActualDurationMinutes),
            AchievementsCount = (await _analyticsService.GetUserAchievementsAsync(userId)).Count
        };
    }

    public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int userId, int count = 10)
    {
        var activities = new List<RecentActivity>();
        
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(userId);
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(userId);
        
        foreach (var session in sessions.Take(count))
        {
            activities.Add(new RecentActivity
            {
                Id = session.Id,
                Type = "workout",
                Title = $"Workout Completed - {session.WorkoutDay?.DayOfWeek}",
                Description = $"Duration: {session.ActualDurationMinutes} minutes. {session.Feeling ?? "Great session!"}",
                Timestamp = session.CreatedAt,
                Icon = ""
            });
        }
        
        foreach (var log in logs.Take(count))
        {
            activities.Add(new RecentActivity
            {
                Id = log.Id,
                Type = "weight",
                Title = "Weight Logged",
                Description = $"Weight: {log.Weight} kg. Body Fat: {log.BodyFatPercentage?.ToString("F1") ?? "N/A"}%",
                Timestamp = log.CreatedAt,
                Icon = " "
            });
        }
        
        return activities.OrderByDescending(a => a.Timestamp).Take(count).ToList();
    }
}