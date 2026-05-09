namespace GymFlow.Services.Interfaces;

/// <summary>
/// Service for exporting gym data to PDF format
/// </summary>
public interface IPdfExportService
{
    /// <summary>
    /// Exports a workout plan to PDF
    /// </summary>
    Task<byte[]> ExportWorkoutPlanToPdfAsync(int workoutPlanId);
    
    /// <summary>
    /// Exports user progress report to PDF
    /// </summary>
    Task<byte[]> ExportProgressReportToPdfAsync(int userId, DateOnly? fromDate = null, DateOnly? toDate = null);
    
    /// <summary>
    /// Exports weekly workout summary to PDF
    /// </summary>
    Task<byte[]> ExportWeeklySummaryToPdfAsync(int userId, DateOnly? weekStart = null);
    
    /// <summary>
    /// Exports user achievements certificate
    /// </summary>
    Task<byte[]> ExportAchievementsCertificateAsync(int userId);
}