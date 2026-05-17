using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;
using GymFlow.Web.Models;

namespace GymFlow.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;

    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public QuickStatsDto? Stats { get; set; }
    public WorkoutDayDetailDto? TodayWorkout { get; set; }
    public List<WeightPointDto>? WeightHistory { get; set; }
    public List<AchievementDto>? Achievements { get; set; }
    public List<RecentActivityDto>? RecentActivities { get; set; }
    public string? ActivePlanName { get; set; }
    
    public string? UserRole { get; set; }
    public string? Username { get; set; }
    public bool IsCoach => UserRole == "Coach";
    public bool IsMember => UserRole == "Member";

    public async Task OnGetAsync()
    {
        // گرفتن userId از Session
        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            Response.Redirect("/Login");
            return;
        }

        UserRole = HttpContext.Session.GetString("UserRole") ?? "Member";
        Username = HttpContext.Session.GetString("Username") ?? "guest";

        if (IsCoach)
        {
            // برای مربی فقط وزن فعلی و روند وزنی را بارگیری می‌کنیم
            await LoadWeightHistoryAsync(userId);
            // سایر بخش‌ها را خالی نگه می‌داریم (یا اصلاً بارگیری نمی‌کنیم)
            Stats = null;
            TodayWorkout = null;
            Achievements = null;
            RecentActivities = null;
            ActivePlanName = null;
        }
        else
        {
            Stats = await _apiClient.GetAsync<QuickStatsDto>($"api/statistics/user/{userId}/quick-stats");
            await LoadTodayWorkoutAsync(userId);
            await LoadWeightHistoryAsync(userId);
            await LoadAchievementsAsync(userId);
            await LoadRecentActivitiesAsync(userId);
            await LoadActivePlanNameAsync(userId);
        }
    }

    private async Task LoadTodayWorkoutAsync(int userId)
    {
        var today = DateTime.Now;
        var todayDayOfWeek = GetPersianDayOfWeekNumber(today);

        var activePlan = await _apiClient.GetAsync<ActivePlanDto>($"api/workoutplans/user/{userId}/active");
        if (activePlan != null && activePlan.Id > 0)
        {
            var workoutDays = await _apiClient.GetAsync<List<WorkoutDayApiDto>>($"api/workoutdays/plan/{activePlan.Id}");
            if (workoutDays != null)
            {
                var todayWorkoutApi = workoutDays.FirstOrDefault(wd => (int)wd.DayOfWeek == todayDayOfWeek);
                if (todayWorkoutApi != null)
                {
                    TodayWorkout = new WorkoutDayDetailDto
                    {
                        DayOfWeek = todayWorkoutApi.DayOfWeek,
                        TargetMuscles = GetMuscleGroupName(todayWorkoutApi.TargetMuscles),
                        DurationMinutes = todayWorkoutApi.DurationMinutes,
                        Intensity = GetIntensityName(todayWorkoutApi.Intensity),
                        DayOfWeekPersian = GetPersianDayName(todayWorkoutApi.DayOfWeek)
                    };
                }
            }
        }
    }

    private async Task LoadWeightHistoryAsync(int userId)
    {
        var logs = await _apiClient.GetAsync<List<ProgressLogDto>>($"api/progress/user/{userId}");
        if (logs != null && logs.Any())
        {
            WeightHistory = logs
                .OrderBy(l => l.LogDate)
                .Select(l => new WeightPointDto
                {
                    Date = l.LogDate,
                    Weight = l.Weight,
                    BodyFatPercentage = l.BodyFatPercentage
                })
                .ToList();
        }
        else
        {
            WeightHistory = new List<WeightPointDto>();
        }
    }

    private async Task LoadAchievementsAsync(int userId)
    {
        Achievements = new List<AchievementDto>();
        var sessions = await _apiClient.GetAsync<List<WorkoutSessionDto>>($"api/workoutsessions/user/{userId}");
        var totalWorkouts = sessions?.Count ?? 0;

        if (totalWorkouts >= 1)
            Achievements.Add(new AchievementDto { Name = "اولین تمرین", Description = "اولین جلسه تمرینی خود را ثبت کردی! 🎯", Icon = "🎯" });
        if (totalWorkouts >= 5)
            Achievements.Add(new AchievementDto { Name = "پایداری", Description = "۵ جلسه تمرین کامل شد! 🌟", Icon = "🌟" });
        if (totalWorkouts >= 10)
            Achievements.Add(new AchievementDto { Name = "شروع قدرتمند", Description = "۱۰ جلسه تمرین کامل شد! ⚡", Icon = "⚡" });
        if (totalWorkouts >= 25)
            Achievements.Add(new AchievementDto { Name = "متعهد به تمرین", Description = "۲۵ جلسه تمرین - عالی! 🔥", Icon = "🔥" });
        if (totalWorkouts >= 50)
            Achievements.Add(new AchievementDto { Name = "ورزشکار حرفه‌ای", Description = "۵۰ جلسه تمرین - فوق‌العاده! 🏆", Icon = "🏆" });
        if (totalWorkouts >= 100)
            Achievements.Add(new AchievementDto { Name = "اسطوره", Description = "۱۰۰ جلسه تمرین! شما یک افسانه هستید 💪", Icon = "💪" });
    }

    private async Task LoadRecentActivitiesAsync(int userId)
    {
        RecentActivities = new List<RecentActivityDto>();

        var sessions = await _apiClient.GetAsync<List<WorkoutSessionDto>>($"api/workoutsessions/user/{userId}");
        if (sessions != null)
        {
            foreach (var session in sessions.OrderByDescending(s => s.CreatedAt).Take(5))
            {
                var feelingText = string.IsNullOrEmpty(session.Feeling) ? "بدون توضیح" : session.Feeling;
                RecentActivities.Add(new RecentActivityDto
                {
                    Title = "تمرین ثبت شد",
                    Description = $"مدت زمان: {session.ActualDurationMinutes} دقیقه - {feelingText}",
                    Timestamp = session.CreatedAt,
                    Icon = "💪",
                    Type = "workout"
                });
            }
        }

        var logs = await _apiClient.GetAsync<List<ProgressLogDto>>($"api/progress/user/{userId}");
        if (logs != null)
        {
            foreach (var log in logs.OrderByDescending(l => l.CreatedAt).Take(3))
            {
                RecentActivities.Add(new RecentActivityDto
                {
                    Title = "وزن ثبت شد",
                    Description = $"وزن: {log.Weight} کیلوگرم" + (log.BodyFatPercentage.HasValue ? $" - چربی: {log.BodyFatPercentage.Value:F1}%" : ""),
                    Timestamp = log.CreatedAt,
                    Icon = "⚖️",
                    Type = "weight"
                });
            }
        }

        RecentActivities = RecentActivities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
    }

    private async Task LoadActivePlanNameAsync(int userId)
    {
        var activePlan = await _apiClient.GetAsync<ActivePlanDto>($"api/workoutplans/user/{userId}/active");
        if (activePlan != null && activePlan.Id > 0)
        {
            ActivePlanName = $"فاز {activePlan.Phase}";
        }
        else
        {
            ActivePlanName = "برنامه فعالی ندارید";
        }
    }

    private int GetPersianDayOfWeekNumber(DateTime date)
    {
        var dotNetDay = date.DayOfWeek;
        return dotNetDay == DayOfWeek.Saturday ? 6 : (int)dotNetDay;
    }

    private string GetPersianDayName(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Saturday => "شنبه",
            DayOfWeek.Sunday => "یکشنبه",
            DayOfWeek.Monday => "دوشنبه",
            DayOfWeek.Tuesday => "سه‌شنبه",
            DayOfWeek.Wednesday => "چهارشنبه",
            DayOfWeek.Thursday => "پنجشنبه",
            DayOfWeek.Friday => "جمعه",
            _ => day.ToString()
        };
    }

    private string GetMuscleGroupName(int muscles)
    {
        var names = new List<string>();
        if ((muscles & 1) != 0) names.Add("Chest");
        if ((muscles & 2) != 0) names.Add("Back");
        if ((muscles & 4) != 0) names.Add("Legs");
        if ((muscles & 8) != 0) names.Add("Shoulders");
        if ((muscles & 16) != 0) names.Add("Arms");
        if ((muscles & 32) != 0) names.Add("Core");
        return names.Count > 0 ? string.Join(", ", names) : "Full Body";
    }

    private string GetIntensityName(int intensity)
    {
        return intensity switch
        {
            0 => "کم",
            1 => "متوسط",
            2 => "زیاد",
            _ => "نامشخص"
        };
    }
}

// ========== DTOهای داخلی ==========

public class QuickStatsDto
{
    public int TotalWorkouts { get; set; }
    public int CurrentStreak { get; set; }
    public int ConsistencyScore { get; set; }
    public float CurrentWeight { get; set; }
}

public class ActivePlanDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public bool IsActive { get; set; }
}

public class WorkoutDayApiDto
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int TargetMuscles { get; set; }
    public int DurationMinutes { get; set; }
    public int Intensity { get; set; }
}

public class WorkoutDayDetailDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public string TargetMuscles { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Intensity { get; set; } = string.Empty;
    public string DayOfWeekPersian { get; set; } = string.Empty;
}

public class WeightPointDto
{
    public DateOnly Date { get; set; }
    public float Weight { get; set; }
    public float? BodyFatPercentage { get; set; }
}

public class AchievementDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class RecentActivityDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class ProgressLogDto
{
    public DateOnly LogDate { get; set; }
    public float Weight { get; set; }
    public float? BodyFatPercentage { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WorkoutSessionDto
{
    public DateOnly ActualDate { get; set; }
    public int ActualDurationMinutes { get; set; }
    public string? Feeling { get; set; }
    public DateTime CreatedAt { get; set; }
}