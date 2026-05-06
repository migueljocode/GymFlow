namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Repository interface for WorkoutSession-specific operations extending the generic repository.
/// </summary>
/// <remarks>
/// Provides specialized query methods for tracking actual completed workout sessions,
/// including date range queries, attendance tracking, and user progress analysis.
/// </remarks>
public interface IWorkoutSessionRepository : IRepository<WorkoutSession>
{
    /// <summary>
    /// Retrieves all completed workout sessions for a user, ordered by date (newest first).
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of workout sessions with their associated workout day info.
    /// </returns>
    /// <remarks>
    /// This eagerly loads the WorkoutDay and its related WorkoutPlan for context.
    /// Useful for displaying a user's complete workout history timeline.
    /// </remarks>
    Task<IEnumerable<WorkoutSession>> GetSessionsByUserAsync(int userId);
    
    /// <summary>
    /// Retrieves all completed sessions for a specific planned workout day template.
    /// </summary>
    /// <param name="workoutDayId">The unique identifier of the workout day template.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of sessions for that day template, ordered by date.
    /// </returns>
    /// <remarks>
    /// Example: Get all times the user actually performed their "Monday Chest Day"
    /// This helps track consistency and performance improvement for specific routines.
    /// </remarks>
    Task<IEnumerable<WorkoutSession>> GetSessionsByWorkoutDayAsync(int workoutDayId);
    
    /// <summary>
    /// Retrieves all workout sessions within a specific date range for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="startDate">The beginning of the date range (inclusive).</param>
    /// <param name="endDate">The end of the date range (inclusive).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of sessions during the specified period, ordered by date.
    /// </returns>
    /// <remarks>
    /// Useful for generating weekly or monthly attendance reports, or analyzing workout frequency.
    /// </remarks>
    Task<IEnumerable<WorkoutSession>> GetSessionsByDateRangeAsync(int userId, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Gets the most recent completed workout session for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the latest session, or <c>null</c> if none exists.
    /// </returns>
    /// <remarks>
    /// Useful for "last workout" displays on dashboards or checking if a user has been active recently.
    /// </remarks>
    Task<WorkoutSession?> GetLatestSessionAsync(int userId);
    
    /// <summary>
    /// Gets the total number of completed workout sessions for a user, optionally from a specific date.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="fromDate">Optional start date. If null, counts all sessions.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the session count.
    /// </returns>
    /// <remarks>
    /// Use for gamification (milestones), attendance percentages, or streak calculations.
    /// </remarks>
    Task<int> GetSessionCountByUserAsync(int userId, DateOnly? fromDate = null);
    
    /// <summary>
    /// Calculates the average duration (in minutes) of all completed workouts for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the average session duration in minutes.
    /// </returns>
    /// <remarks>
    /// Helps coaches understand user engagement and workout intensity. A decreasing average
    /// might indicate fatigue or lack of motivation, while increasing suggests progress.
    /// </remarks>
    Task<double> GetAverageSessionDurationAsync(int userId);
    
    /// <summary>
    /// Checks whether a user has already completed a specific workout day on a given date.
    /// </summary>
    /// <param name="workoutDayId">The workout day template ID.</param>
    /// <param name="date">The date to check.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if the user completed that day's workout on the specified date; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Prevents duplicate logging of the same workout on the same day.
    /// Also useful for marking workout days as "completed" in calendar views.
    /// </remarks>
    Task<bool> HasUserCompletedWorkoutDayAsync(int workoutDayId, DateOnly date);
}