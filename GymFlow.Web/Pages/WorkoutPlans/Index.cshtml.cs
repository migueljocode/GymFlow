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
    public bool IsViewingOwnPlans { get; set; } = true;
    public int? ClientId { get; set; }

    public async Task<IActionResult> OnGetAsync(int? userId = null)
    {
        int targetUserId;
        bool isViewingOwn;

        if (IsCoach && userId.HasValue && userId.Value > 0)
        {
            targetUserId = userId.Value;
            isViewingOwn = false;
            ClientId = userId.Value;
        }
        else
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out targetUserId))
                return RedirectToPage("/Login");
            isViewingOwn = true;
            ClientId = null;
        }

        IsViewingOwnPlans = isViewingOwn;
        WorkoutPlans = await _apiClient.GetAsync<List<WorkoutPlanListDto>>($"api/workoutplans/user/{targetUserId}") ?? new();
        return Page();
    }
}

// DTO مورد استفاده در Index (اگر خارج از این فایل تعریف نشده باشد)
public class WorkoutPlanListDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public int SessionsPerWeek { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
}