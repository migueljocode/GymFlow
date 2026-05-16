using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;

namespace GymFlow.Web.Pages.Reports;

public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    
    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public int ActivePlanId { get; set; }
    public int CurrentUserId { get; set; }
        
    public async Task<IActionResult> OnGetAsync()
    {
        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            return RedirectToPage("/Login");
        }

        CurrentUserId = userId;
        var activePlan = await _apiClient.GetAsync<ActivePlanDto>($"api/workoutplans/user/{CurrentUserId}/active");
        ActivePlanId = activePlan?.Id ?? 0;
        return Page();
    }
    
    public async Task<IActionResult> OnPostDownloadWorkoutPlanAsync(int planId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/workout-plan/{planId}");
        if (pdfBytes == null)
            return NotFound();
        
        return File(pdfBytes, "application/pdf", $"WorkoutPlan_{planId}.pdf");
    }
    
    public async Task<IActionResult> OnPostDownloadProgressAsync(int userId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/progress/{userId}");
        if (pdfBytes == null)
            return NotFound();
        
        return File(pdfBytes, "application/pdf", $"ProgressReport_User_{userId}.pdf");
    }
    
    public async Task<IActionResult> OnPostDownloadCertificateAsync(int userId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/certificate/{userId}");
        if (pdfBytes == null)
            return NotFound();
        
        return File(pdfBytes, "application/pdf", $"Certificate_User_{userId}.pdf");
    }
    
    public async Task<IActionResult> OnPostDownloadWeeklySummaryAsync(int userId, string weekStart)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/weekly-summary/{userId}?weekStart={weekStart}");
        if (pdfBytes == null)
            return NotFound();
        
        return File(pdfBytes, "application/pdf", $"WeeklySummary_User_{userId}.pdf");
    }
}

public class ActivePlanDto
{
    public int Id { get; set; }
    public int Phase { get; set; }
    public bool IsActive { get; set; }
}