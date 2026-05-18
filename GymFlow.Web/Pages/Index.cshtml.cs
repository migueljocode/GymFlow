namespace GymFlow.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;

    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public QuickStatsResponse? Stats { get; set; }
    public CoachStatsResponse? CoachStats { get; set; }
    public TodayWorkoutResponse? TodayWorkout { get; set; }
    public List<WeightPointResponse>? WeightHistory { get; set; }
    public List<AchievementResponse>? Achievements { get; set; }
    public List<RecentActivityResponse>? RecentActivities { get; set; }
    public string? ActivePlanName { get; set; }
    
    public string? UserRole { get; set; }
    public string? Username { get; set; }
    public bool IsCoach => UserRole == "Coach";
    public bool IsMember => UserRole == "Member";

    public async Task OnGetAsync()
    {
        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            Response.Redirect("/Login");
            return;
        }

        UserRole = HttpContext.Session.GetString("UserRole") ?? "Member";
        Username = HttpContext.Session.GetString("Username") ?? "guest";

        if (IsCoach)
        {
            CoachStats = await _apiClient.GetAsync<CoachStatsResponse>($"api/statistics/coach/{userId}/dashboard");
            await LoadWeightHistoryAsync(userId);
            RecentActivities = await _apiClient.GetAsync<List<RecentActivityResponse>>($"api/statistics/coach/{userId}/recent-activities");
        }
        else
        {
            Stats = await _apiClient.GetAsync<QuickStatsResponse>($"api/statistics/user/{userId}/quick-stats");
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

        var activePlan = await _apiClient.GetAsync<ActivePlanResponse>($"api/workoutplans/user/{userId}/active");
        if (activePlan != null && activePlan.Id > 0)
        {
            var workoutDays = await _apiClient.GetAsync<List<WorkoutDayResponse>>($"api/workoutdays/plan/{activePlan.Id}");
            if (workoutDays != null)
            {
                var todayWorkoutApi = workoutDays.FirstOrDefault(wd => (int)wd.DayOfWeek == todayDayOfWeek);
                if (todayWorkoutApi != null)
                {
                    TodayWorkout = new TodayWorkoutResponse
                    {
                        DayOfWeek = todayWorkoutApi.DayOfWeek,
                        TargetMuscles = GetMuscleGroupName((int)todayWorkoutApi.TargetMuscles),
                        DurationMinutes = todayWorkoutApi.DurationMinutes,
                        Intensity = GetIntensityName((int)todayWorkoutApi.Intensity),
                        DayOfWeekPersian = GetPersianDayName(todayWorkoutApi.DayOfWeek)
                    };
                }
            }
        }
    }

    private async Task LoadWeightHistoryAsync(int userId)
    {
        var logs = await _apiClient.GetAsync<List<ProgressLogResponse>>($"api/progress/user/{userId}");
        if (logs != null && logs.Any())
        {
            WeightHistory = logs
                .OrderBy(l => l.LogDate)
                .Select(l => new WeightPointResponse
                {
                    Date = l.LogDate,
                    Weight = l.Weight,
                    BodyFatPercentage = l.BodyFatPercentage
                })
                .ToList();
        }
        else
        {
            WeightHistory = new List<WeightPointResponse>();
        }
    }

    private async Task LoadAchievementsAsync(int userId)
    {
        Achievements = new List<AchievementResponse>();
        var sessions = await _apiClient.GetAsync<List<WorkoutSessionResponse>>($"api/workoutsessions/user/{userId}");
        var totalWorkouts = sessions?.Count ?? 0;

        if (totalWorkouts >= 1)
            Achievements.Add(new AchievementResponse { Name = "اولین تمرین", Description = "اولین جلسه تمرینی خود را ثبت کردی!", Icon = "fa fa-star" });
        if (totalWorkouts >= 5)
            Achievements.Add(new AchievementResponse { Name = "پایداری", Description = "5 جلسه تمرین کامل شد!", Icon = "fa fa-fire" });
        if (totalWorkouts >= 10)
            Achievements.Add(new AchievementResponse { Name = "شروع قدرتمند", Description = "10 جلسه تمرین کامل شد!", Icon = "fa fa-bolt" });
        if (totalWorkouts >= 25)
            Achievements.Add(new AchievementResponse { Name = "متعهد به تمرین", Description = "25 جلسه تمرین - عالی!", Icon = "fa fa-trophy" });
        if (totalWorkouts >= 50)
            Achievements.Add(new AchievementResponse { Name = "ورزشکار حرفه‌ای", Description = "50 جلسه تمرین - فوق‌العاده!", Icon = "fa fa-crown" });
        if (totalWorkouts >= 100)
            Achievements.Add(new AchievementResponse { Name = "اسطوره", Description = "100 جلسه تمرین! شما یک افسانه هستید!", Icon = "fa fa-gem" });
    }

    private async Task LoadRecentActivitiesAsync(int userId)
    {
        RecentActivities = new List<RecentActivityResponse>();

        var sessions = await _apiClient.GetAsync<List<WorkoutSessionResponse>>($"api/workoutsessions/user/{userId}");
        if (sessions != null)
        {
            foreach (var session in sessions.OrderByDescending(s => s.CreatedAt).Take(5))
            {
                var feelingText = string.IsNullOrEmpty(session.Feeling) ? "بدون توضیح" : session.Feeling;
                RecentActivities.Add(new RecentActivityResponse
                {
                    Title = "تمرین ثبت شد",
                    Description = $"مدت زمان: {session.ActualDurationMinutes} دقیقه - {feelingText}",
                    Timestamp = session.CreatedAt,
                    Icon = "fa fa-dumbbell",
                    Type = "workout"
                });
            }
        }

        var logs = await _apiClient.GetAsync<List<ProgressLogResponse>>($"api/progress/user/{userId}");
        if (logs != null)
        {
            foreach (var log in logs.OrderByDescending(l => l.CreatedAt).Take(3))
            {
                RecentActivities.Add(new RecentActivityResponse
                {
                    Title = "وزن ثبت شد",
                    Description = $"وزن: {log.Weight} کیلوگرم" + (log.BodyFatPercentage.HasValue ? $" - چربی: {log.BodyFatPercentage.Value:F1}%" : ""),
                    Timestamp = log.CreatedAt,
                    Icon = "fa fa-weight-scale",
                    Type = "weight"
                });
            }
        }

        RecentActivities = RecentActivities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
    }

    private async Task LoadActivePlanNameAsync(int userId)
    {
        var activePlan = await _apiClient.GetAsync<ActivePlanResponse>($"api/workoutplans/user/{userId}/active");
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