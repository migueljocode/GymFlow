using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;
using GymFlow.Models.DTOs.Requests;

namespace GymFlow.Web.Pages.Workout;

public class LogModel : PageModel
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
    
    public async Task<IActionResult> OnPostAsync()
    {
        // گرفتن اولین کاربر
        var users = await _apiClient.GetAsync<List<UserDto>>("api/users");
        var userId = users?.FirstOrDefault()?.Id ?? 1;
        
        var todayWorkout = await _apiClient.GetAsync<dynamic>($"api/workoutplans/user/{userId}/today");
        var workoutDayId = todayWorkout?.workoutDay?.id ?? 0;
        
        if (workoutDayId == 0)
        {
            Message = "امروز برنامه تمرینی ندارید!";
            return Page();
        }
        
        var request = new LogWorkoutRequest
        {
            WorkoutDayId = workoutDayId,
            ActualDate = ActualDate,
            ActualDurationMinutes = DurationMinutes,
            Feeling = Feeling
        };
        
        var result = await _apiClient.PostAsync<object>("api/workoutsessions/log", request);
        
        if (result != null)
        {
            Message = "تمرین با موفقیت ثبت شد! 🔥";
            return Page();
        }
        
        Message = "خطا در ثبت تمرین. دوباره تلاش کن.";
        return Page();
    }
}

public class UserDto
{
    public int Id { get; set; }
}