using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.Enums;

namespace GymFlow.Web.Pages.WorkoutPlans;

public class CreateModel : PageModel
{
    private readonly ApiClient _apiClient;
    
    public CreateModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    [BindProperty]
    public int Phase { get; set; } = 1;
    
    [BindProperty]
    public int SessionsPerWeek { get; set; } = 3;
    
    [BindProperty]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    [BindProperty]
    public DateOnly? EndDate { get; set; }
    
    [BindProperty]
    public string? Notes { get; set; }
    
    [BindProperty]
    public List<int> SelectedDays { get; set; } = new();
    
    public Dictionary<string, int> AvailableDays { get; set; } = new()
    {
        { "شنبه", 6 },
        { "یکشنبه", 0 },
        { "دوشنبه", 1 },
        { "سه‌شنبه", 2 },
        { "چهارشنبه", 3 },
        { "پنجشنبه", 4 },
        { "جمعه", 5 }
    };
    
    public string? ErrorMessage { get; set; }
    
    public async Task OnGetAsync()
    {
        var users = await _apiClient.GetAsync<List<UserInfoDto>>("api/users");
        var userId = users?.FirstOrDefault()?.Id ?? 1;
        
        var existingPlans = await _apiClient.GetAsync<List<WorkoutPlanInfoDto>>($"api/workoutplans/user/{userId}");
        if (existingPlans != null && existingPlans.Any())
        {
            var maxPhase = existingPlans.Max(p => p.Phase);
            Phase = maxPhase + 1;
        }
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "اطلاعات وارد شده معتبر نیست";
            return Page();
        }
        
        if (SelectedDays.Count == 0)
        {
            ErrorMessage = "حداقل یک روز تمرینی را انتخاب کنید";
            return Page();
        }
        
        var users = await _apiClient.GetAsync<List<UserInfoDto>>("api/users");
        var userId = users?.FirstOrDefault()?.Id ?? 1;
        
        var existingPlans = await _apiClient.GetAsync<List<WorkoutPlanInfoDto>>($"api/workoutplans/user/{userId}");
        if (existingPlans != null && existingPlans.Any(p => p.Phase == Phase))
        {
            ErrorMessage = $"فاز {Phase} قبلاً ایجاد شده است!";
            return Page();
        }
        
        var activePlan = existingPlans?.FirstOrDefault(p => p.IsActive);
        if (activePlan != null)
        {
            await _apiClient.PostAsync<object>($"api/workoutplans/{activePlan.Id}/deactivate", new { });
        }
        
        var request = new CreateWorkoutPlanRequest
        {
            UserId = userId,
            Phase = Phase,
            SessionsPerWeek = SessionsPerWeek,
            StartDate = StartDate,
            EndDate = EndDate,
            Notes = Notes
        };
        
        var plan = await _apiClient.PostAsync<WorkoutPlanCreatedDto>("api/workoutplans", request);
        
        if (plan == null || plan.Id == 0)
        {
            ErrorMessage = "خطا در ایجاد برنامه تمرینی";
            return Page();
        }
        
        foreach (var dayValue in SelectedDays)
        {
            var dayRequest = new CreateWorkoutDayRequest
            {
                WorkoutPlanId = plan.Id,
                DayOfWeek = (DayOfWeek)dayValue,
                TargetMuscles = MuscleGroup.None,
                DurationMinutes = 60,
                Intensity = Intensity.Medium,
                Notes = null
            };
            
            await _apiClient.PostAsync<object>("api/workoutdays", dayRequest);
        }
        
        // رفتن به صفحه ویرایش برنامه (AddExercises)
        var firstWorkoutDay = await _apiClient.GetAsync<List<WorkoutDayDto>>($"api/workoutdays/plan/{plan.Id}");
        if (firstWorkoutDay != null && firstWorkoutDay.Any())
        {
            return RedirectToPage("/WorkoutPlans/AddExercises", new { 
                workoutDayId = firstWorkoutDay.First().Id, 
                workoutPlanId = plan.Id, 
                dayOfWeek = firstWorkoutDay.First().DayOfWeek.ToString() 
            });
        }
        
        return RedirectToPage("/WorkoutPlans/Details", new { id = plan.Id });
    }
}

public class UserInfoDto
{
    public int Id { get; set; }
}

public class WorkoutPlanInfoDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public bool IsActive { get; set; }
}

public class WorkoutPlanCreatedDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
}

public class WorkoutDayDto
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}