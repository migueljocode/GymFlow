namespace GymFlow.Web.Pages.WorkoutPlans;

public class DetailsModel : BasePageModel
{
    private readonly ApiClient _apiClient;

    public DetailsModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public WorkoutPlanDetailsResponse? Plan { get; set; }
    public int CompletedSessions { get; set; }
    public int TotalSessions { get; set; }
    public int CompletionPercentage { get; set; }
    public int ClientId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, int? userId = null)
    {
        if (IsCoach && userId.HasValue && userId.Value > 0)
        {
            ClientId = userId.Value;
        }
        else
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var currentUserId))
                return RedirectToPage("/Login");
            ClientId = currentUserId;
        }

        Plan = await _apiClient.GetAsync<WorkoutPlanDetailsResponse>($"api/workoutplans/{id}/details");

        if (Plan != null && Plan.WorkoutDays != null)
        {
            TotalSessions = Plan.WorkoutDays.Count * 4;
            CompletedSessions = 0;
            CompletionPercentage = TotalSessions > 0 ? (CompletedSessions * 100 / TotalSessions) : 0;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDownloadPdfAsync(int planId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/workout-plan/{planId}");
        if (pdfBytes == null) return NotFound();
        return File(pdfBytes, "application/pdf", $"WorkoutPlan_{planId}.pdf");
    }
}