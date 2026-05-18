namespace GymFlow.Web.Pages.WorkoutPlans;

public class IndexModel : BasePageModel
{
    private readonly ApiClient _apiClient;

    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<WorkoutPlanListResponse> WorkoutPlans { get; set; } = new();
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
        WorkoutPlans = await _apiClient.GetAsync<List<WorkoutPlanListResponse>>($"api/workoutplans/user/{targetUserId}") ?? new();
        return Page();
    }
}