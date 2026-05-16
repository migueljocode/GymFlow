using Microsoft.AspNetCore.Mvc;
using GymFlow.Web.Services;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Web.Pages.WorkoutPlans;

namespace GymFlow.Web.Pages.Workout;

public class LogModel : BasePageModel   // تغییر ارث‌بری
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
    
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        // اعمال محدودیت دسترسی
        var redirect = RedirectIfNotMember();
        if (redirect != null) return redirect;

        // گرفتن userId از Session
        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            ErrorMessage = "لطفاً مجدداً وارد شوید.";
            return RedirectToPage("/Login");
        }
        
        var dayOfWeek = ActualDate.DayOfWeek;
        var activePlan = await _apiClient.GetAsync<ActivePlanDto>($"api/workoutplans/user/{userId}/active");
        
        if (activePlan == null || activePlan.Id == 0)
        {
            ErrorMessage = "❌ برنامه تمرینی فعالی ندارید! لطفاً ابتدا یک برنامه تمرینی ایجاد کنید.";
            return Page();
        }
        
        var workoutDays = await _apiClient.GetAsync<List<WorkoutDayDto>>($"api/workoutdays/plan/{activePlan.Id}");
        var targetDay = workoutDays?.FirstOrDefault(wd => wd.DayOfWeek == dayOfWeek);
        
        if (targetDay == null)
        {
            ErrorMessage = $"❌ برای روز {dayOfWeek} برنامه تمرینی ندارید!";
            return Page();
        }
        
        var request = new LogWorkoutRequest
        {
            WorkoutDayId = targetDay.Id,
            ActualDate = ActualDate,
            ActualDurationMinutes = DurationMinutes,
            Feeling = Feeling
        };
        
        try
        {
            var result = await _apiClient.PostAsync<object>("api/workoutsessions/log", request);
            
            if (result != null)
            {
                Message = $"✅ تمرین برای تاریخ {ActualDate} با موفقیت ثبت شد! 🔥";
                return Page();
            }
            
            ErrorMessage = "❌ خطا در ثبت تمرین. دوباره تلاش کن.";
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("409") || ex.Message.Contains("Conflict"))
            {
                ErrorMessage = $"⚠️ تمرین برای تاریخ {ActualDate} قبلاً ثبت شده است! نمی‌توانید دوباره ثبت کنید.";
            }
            else
            {
                ErrorMessage = $"❌ خطا در ثبت تمرین: {ex.Message}";
            }
        }
        
        return Page();
    }
}