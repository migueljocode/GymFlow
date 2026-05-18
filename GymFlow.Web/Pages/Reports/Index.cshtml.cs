namespace GymFlow.Web.Pages.Reports;

public class IndexModel : BasePageModel
{
    private readonly ApiClient _apiClient;
    
    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public int ActivePlanId { get; set; }
    public int CurrentUserId { get; set; }
    public List<ClientInfoResponse> Clients { get; set; } = new();
    public int SelectedClientId { get; set; }
    
    public async Task<IActionResult> OnGetAsync(int? clientId)
    {
        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            return RedirectToPage("/Login");

        CurrentUserId = userId;

        if (IsCoach)
        {
            Clients = await _apiClient.GetAsync<List<ClientInfoResponse>>($"api/coaches/{userId}/clients") ?? new();
            if (clientId.HasValue && Clients.Any(c => c.Id == clientId.Value))
                SelectedClientId = clientId.Value;
            else if (Clients.Any())
                SelectedClientId = Clients.First().Id;

            if (SelectedClientId > 0)
            {
                var activePlan = await _apiClient.GetAsync<ActivePlanResponse>($"api/workoutplans/user/{SelectedClientId}/active");
                ActivePlanId = activePlan?.Id ?? 0;
            }
        }
        else
        {
            var activePlan = await _apiClient.GetAsync<ActivePlanResponse>($"api/workoutplans/user/{userId}/active");
            ActivePlanId = activePlan?.Id ?? 0;
        }
        return Page();
    }
    
    public async Task<IActionResult> OnPostDownloadWorkoutPlanAsync(int planId, int? userId)
    {
        int targetUserId = userId ?? (IsCoach ? SelectedClientId : CurrentUserId);
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/workout-plan/{planId}");
        if (pdfBytes == null) return NotFound();
        return File(pdfBytes, "application/pdf", $"WorkoutPlan_{planId}.pdf");
    }
    
    public async Task<IActionResult> OnPostDownloadProgressAsync(int userId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/progress/{userId}");
        if (pdfBytes == null) return NotFound();
        return File(pdfBytes, "application/pdf", $"ProgressReport_User_{userId}.pdf");
    }
    
    public async Task<IActionResult> OnPostDownloadCertificateAsync(int userId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/certificate/{userId}");
        if (pdfBytes == null) return NotFound();
        return File(pdfBytes, "application/pdf", $"Certificate_User_{userId}.pdf");
    }
    
    public async Task<IActionResult> OnPostDownloadWeeklySummaryAsync(int userId, string weekStart)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/weekly-summary/{userId}?weekStart={weekStart}");
        if (pdfBytes == null) return NotFound();
        return File(pdfBytes, "application/pdf", $"WeeklySummary_User_{userId}.pdf");
    }
}