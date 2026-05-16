using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;

namespace GymFlow.Web.Pages.WorkoutPlans;

public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    
    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public List<WorkoutPlanListDto> WorkoutPlans { get; set; } = new();
    
    public async Task OnGetAsync()
    {
        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            Response.Redirect("/Login");
            return;
        }
        
        WorkoutPlans = await _apiClient.GetAsync<List<WorkoutPlanListDto>>($"api/workoutplans/user/{userId}") ?? new();
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