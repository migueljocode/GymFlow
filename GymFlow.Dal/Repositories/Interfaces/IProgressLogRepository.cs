namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Repository interface for ProgressLog-specific operations extending the generic repository.
/// </summary>
/// <remarks>
/// Provides specialized query methods for tracking user progress metrics including weight,
/// body fat percentage, and performance trends over time.
/// </remarks>
public interface IProgressLogRepository : IRepository<ProgressLog>
{
    /// <summary>
    /// Retrieves the complete progress history for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of progress logs ordered by date (newest first).
    /// </returns>
    /// <remarks>
    /// Includes weight, body fat percentage, and any notes recorded during each log entry.
    /// Useful for displaying weight history charts in the dashboard.
    /// </remarks>
    Task<IEnumerable<ProgressLog>> GetUserProgressHistoryAsync(int userId);
    
    /// <summary>
    /// Retrieves user progress logs filtered by workout plan.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="planId">Optional workout plan ID. If null, returns logs not associated with any plan (e.g., during breaks).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of progress logs ordered by date (oldest first).
    /// </returns>
    /// <remarks>
    /// This helps analyze how effective each workout plan was for the user's progress.
    /// Logs during breaks (planId = null) show weight gain/inactivity impact.
    /// </remarks>
    Task<IEnumerable<ProgressLog>> GetUserProgressByPlanAsync(int userId, int? planId = null);
    
    /// <summary>
    /// Retrieves the most recent progress log entry for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the latest progress log, or <c>null</c> if none exists.
    /// </returns>
    /// <remarks>
    /// Useful for displaying current weight and body stats on user dashboard without loading full history.
    /// </remarks>
    Task<ProgressLog?> GetLatestProgressLogAsync(int userId);
    
    /// <summary>
    /// Retrieves a progress log entry for a specific date.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="date">The date of the progress log to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the progress log for that date, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// Ensures a user doesn't log multiple entries on the same date (unique constraint enforced by database).
    /// </remarks>
    Task<ProgressLog?> GetProgressLogByDateAsync(int userId, DateOnly date);
    
    /// <summary>
    /// Retrieves weight trend data for a user over the last N entries.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="lastNEntries">The number of most recent entries to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the last N entries ordered chronologically (oldest to newest).
    /// </returns>
    /// <remarks>
    /// This is specifically designed for weight prediction algorithms and chart generation.
    /// The chronological ordering makes it easy to calculate trends and rate of change.
    /// </remarks>
    Task<IEnumerable<ProgressLog>> GetWeightTrendAsync(int userId, int lastNEntries);
    
    /// <summary>
    /// Calculates average weekly weight change (weight loss/gain rate) over a specified period.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="weeks">The number of weeks to look back (default 4).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains average weekly weight change in kg/week (positive = gain, negative = loss).
    /// Returns <c>null</c> if insufficient data (less than 2 logs in the period).
    /// </returns>
    /// <remarks>
    /// Example: -0.5 kg/week means user is losing half a kilo per week on average.
    /// This is more accurate for long-term trends than day-to-day fluctuations.
    /// </remarks>
    Task<float?> GetAverageWeeklyProgressAsync(int userId, int weeks = 4);
    
    /// <summary>
    /// Calculates the weight difference between two specific dates.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="fromDate">The starting date.</param>
    /// <param name="toDate">The ending date.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains weight change (positive = gain, negative = loss).
    /// Returns <c>null</c> if log entries for either date are missing.
    /// </returns>
    /// <remarks>
    /// Useful for comparing "before and after" for specific time periods like:
    /// "How much weight did you lose in January?" or "Progress during Phase 2 of the plan."
    /// </remarks>
    Task<float?> GetWeightDifferenceAsync(int userId, DateOnly fromDate, DateOnly toDate);
}