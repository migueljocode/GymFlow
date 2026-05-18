namespace GymFlow.Api.Controllers;

[Tags("Export")]
public class ExportController : ApiControllerBase
{
    private readonly IPdfExportService _pdfExportService;

    public ExportController(IPdfExportService pdfExportService)
    {
        _pdfExportService = pdfExportService;
    }

    /// <summary>
    /// دانلود برنامه تمرینی به صورت PDF
    /// </summary>
    [HttpGet("workout-plan/{planId:int}")]
    public async Task<IActionResult> ExportWorkoutPlanAsync(int planId)
    {
        var pdfBytes = await _pdfExportService.ExportWorkoutPlanToPdfAsync(planId);
        return File(pdfBytes, "application/pdf", $"WorkoutPlan_{planId}.pdf");
    }

    /// <summary>
    /// دانلود گزارش پیشرفت کاربر به صورت PDF
    /// </summary>
    [HttpGet("progress/{userId:int}")]
    public async Task<IActionResult> ExportProgressReportAsync(int userId, [FromQuery] DateOnly? fromDate = null, [FromQuery] DateOnly? toDate = null)
    {
        var pdfBytes = await _pdfExportService.ExportProgressReportToPdfAsync(userId, fromDate, toDate);
        return File(pdfBytes, "application/pdf", $"ProgressReport_User_{userId}.pdf");
    }

    /// <summary>
    /// دانلود خلاصه هفتگی به صورت PDF
    /// </summary>
    [HttpGet("weekly-summary/{userId:int}")]
    public async Task<IActionResult> ExportWeeklySummaryAsync(int userId, [FromQuery] DateOnly? weekStart = null)
    {
        var pdfBytes = await _pdfExportService.ExportWeeklySummaryToPdfAsync(userId, weekStart);
        return File(pdfBytes, "application/pdf", $"WeeklySummary_User_{userId}.pdf");
    }

    /// <summary>
    /// دانلود گواهی دستاوردها به صورت PDF
    /// </summary>
    [HttpGet("certificate/{userId:int}")]
    public async Task<IActionResult> ExportCertificateAsync(int userId)
    {
        var pdfBytes = await _pdfExportService.ExportAchievementsCertificateAsync(userId);
        return File(pdfBytes, "application/pdf", $"Certificate_User_{userId}.pdf");
    }
}