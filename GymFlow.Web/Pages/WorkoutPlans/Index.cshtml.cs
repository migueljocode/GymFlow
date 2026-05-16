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
    
    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfNotMember();
        if (redirect != null) return redirect;

        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            return RedirectToPage("/Login");
        
        WorkoutPlans = await _apiClient.GetAsync<List<WorkoutPlanListDto>>($"api/workoutplans/user/{userId}") ?? new();
        return Page();
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