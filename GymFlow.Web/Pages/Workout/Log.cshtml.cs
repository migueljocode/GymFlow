namespace GymFlow.Web.Pages.Workout;

public class LogModel : BasePageModel
{
    private readonly ApiClient _apiClient;
    
    public LogModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    [BindProperty]
    public DateOnly ActualDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    [BindProperty]
    public int DurationMinutes { get; set; } = 45;
    
    [BindProperty]
    public string? Feeling { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var redirect = RedirectIfNotMember();
        if (redirect != null) return redirect;

        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            TempData["ErrorMessage"] = "لطفاً مجدداً وارد شوید.";
            return RedirectToPage("/Login");
        }
        
        if (DurationMinutes < 1 || DurationMinutes > 300)
        {
            TempData["ErrorMessage"] = "مدت زمان باید بین ۱ تا ۳۰۰ دقیقه باشد.";
            return RedirectToPage();
        }
        
        var dayOfWeek = ActualDate.DayOfWeek;
        var activePlan = await _apiClient.GetAsync<ActivePlanResponse>($"api/workoutplans/user/{userId}/active");
        
        if (activePlan == null || activePlan.Id == 0)
        {
            TempData["ErrorMessage"] = "❌ برنامه تمرینی فعالی ندارید! لطفاً با مربی خود تماس بگیرید.";
            return RedirectToPage();
        }
        
        var workoutDays = await _apiClient.GetAsync<List<WorkoutDayResponse>>($"api/workoutdays/plan/{activePlan.Id}");
        var targetDay = workoutDays?.FirstOrDefault(wd => wd.DayOfWeek == dayOfWeek);
        
        if (targetDay == null)
        {
            TempData["ErrorMessage"] = $"❌ برای روز {GetPersianDayName(dayOfWeek)} برنامه تمرینی ندارید!";
            return RedirectToPage();
        }
        
        var request = new LogWorkoutRequest
        {
            WorkoutDayId = targetDay.Id,
            ActualDate = ActualDate,
            ActualDurationMinutes = DurationMinutes,
            Feeling = Feeling
        };
        
        var (result, errorMessage) = await _apiClient.PostWithErrorAsync<object>("api/workoutsessions/log", request);
        
        if (result != null)
        {
            TempData["Message"] = $"✅ تمرین برای تاریخ {ActualDate.ToString("yyyy/MM/dd")} با موفقیت ثبت شد! ";
        }
        else
        {
            TempData["ErrorMessage"] = errorMessage ?? "❌ خطا در ثبت تمرین. لطفاً دوباره تلاش کنید.";
        }
        
        return RedirectToPage();
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
}