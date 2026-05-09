using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;

namespace GymFlow.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    
    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public QuickStatsDto? Stats { get; set; }
    public WorkoutDayDto? TodayWorkout { get; set; }
    public List<WeightPointDto>? WeightHistory { get; set; }
    public List<AchievementDto>? Achievements { get; set; }
    
    public async Task OnGetAsync()
    {
        var userId = 1; // موقتاً از دمو یوزر استفاده می‌کنیم
        
        Stats = await _apiClient.GetAsync<QuickStatsDto>($"api/statistics/user/{userId}/quick-stats");
        TodayWorkout = await _apiClient.GetAsync<WorkoutDayDto>($"api/workoutplans/user/{userId}/today");
        WeightHistory = await _apiClient.GetAsync<List<WeightPointDto>>($"api/progress/user/{userId}/weight-history");
        Achievements = await _apiClient.GetAsync<List<AchievementDto>>($"api/statistics/user/{userId}/achievements");
    }
}

// DTOهای ساده برای نمایش
public class QuickStatsDto
{
    public int TotalWorkouts { get; set; }
    public int CurrentStreak { get; set; }
    public int ConsistencyScore { get; set; }
    public float CurrentWeight { get; set; }
}

public class WorkoutDayDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public string TargetMuscles { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}

public class WeightPointDto
{
    public DateOnly Date { get; set; }
    public float Weight { get; set; }
}

public class AchievementDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}