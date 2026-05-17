using Microsoft.AspNetCore.Mvc;
using GymFlow.Web.Services;

namespace GymFlow.Web.Pages.WorkoutPlans;

public class IndexModel : BasePageModel
{
    private readonly ApiClient _apiClient;

    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<WorkoutPlanListDto> WorkoutPlans { get; set; } = new();
    public int? ClientId { get; set; }   // برای مربی، id مشتری جاری

    public async Task<IActionResult> OnGetAsync(int? userId = null)
    {
        if (IsCoach)
        {
            // مربی حتماً باید userId یک مشتری را دریافت کند
            if (!userId.HasValue || userId.Value == 0)
                return RedirectToPage("/Coach/Clients");

            ClientId = userId.Value;
            WorkoutPlans = await _apiClient.GetAsync<List<WorkoutPlanListDto>>($"api/workoutplans/user/{ClientId}") ?? new();
            return Page();
        }
        else // عضو عادی
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var currentUserId))
                return RedirectToPage("/Login");

            WorkoutPlans = await _apiClient.GetAsync<List<WorkoutPlanListDto>>($"api/workoutplans/user/{currentUserId}") ?? new();
            return Page();
        }
    }
}

public class WorkoutPlanListDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public int SessionsPerWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
}