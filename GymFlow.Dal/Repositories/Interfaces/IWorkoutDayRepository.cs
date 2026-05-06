namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Repository interface for WorkoutDay-specific operations extending the generic repository.
/// </summary>
/// <remarks>
/// Provides specialized query methods for managing workout days within workout plans,
/// including exercise details, session history, and weekday-based queries.
/// </remarks>
public interface IWorkoutDayRepository : IRepository<WorkoutDay>
{
    /// <summary>
    /// Retrieves all workout days for a specific workout plan, ordered by day of week.
    /// </summary>
    /// <param name="planId">The unique identifier of the workout plan.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of workout days belonging to the plan, ordered from Monday to Sunday.
    /// </returns>
    /// <remarks>
    /// This returns only the day templates (planned workouts), not actual completed sessions.
    /// Useful for displaying the weekly workout schedule template.
    /// </remarks>
    Task<IEnumerable<WorkoutDay>> GetWorkoutDaysByPlanAsync(int planId);
    
    /// <summary>
    /// Retrieves a workout day with all its exercises included (sets, reps, detailed instructions).
    /// </summary>
    /// <param name="workoutDayId">The unique identifier of the workout day.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the workout day with full exercise details, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// This eagerly loads:
    /// <list type="bullet">
    /// <item><description>WorkoutDayExercises (sets, reps, rest)</description></item>
    /// <item><description>Exercise library data (name, muscle group)</description></item>
    /// </list>
    /// Use this when displaying the full workout routine for a specific day.
    /// </remarks>
    Task<WorkoutDay?> GetWorkoutDayWithExercisesAsync(int workoutDayId);
    
    /// <summary>
    /// Retrieves a workout day with all its completed sessions (history of when this day was actually performed).
    /// </summary>
    /// <param name="workoutDayId">The unique identifier of the workout day.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the workout day with session history, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// Useful for tracking how many times a user has completed this specific day's workout,
    /// and viewing their performance trends over time.
    /// </remarks>
    Task<WorkoutDay?> GetWorkoutDayWithSessionsAsync(int workoutDayId);
    
    /// <summary>
    /// Retrieves all workout days that occur on a specific day of the week across all plans.
    /// </summary>
    /// <param name="dayOfWeek">The day of week (Monday, Tuesday, etc.).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of workout days scheduled on that weekday.
    /// </returns>
    /// <remarks>
    /// Useful for finding all Monday workouts across all users (for statistical analysis or group classes).
    /// </remarks>
    Task<IEnumerable<WorkoutDay>> GetWorkoutDaysByWeekdayAsync(DayOfWeek dayOfWeek);
    
    /// <summary>
    /// Retrieves the workout day for a specific plan on a specific day of the week.
    /// </summary>
    /// <param name="planId">The unique identifier of the workout plan.</param>
    /// <param name="dayOfWeek">The day of week to search for.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the matching workout day, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// Since a plan should have at most one workout day per weekday (unique constraint enforced),
    /// this method is useful for ensuring no duplicate days or retrieving the planned workout for "today".
    /// </remarks>
    Task<WorkoutDay?> GetWorkoutDayByWeekdayAndPlanAsync(int planId, DayOfWeek dayOfWeek);
    
    /// <summary>
    /// Gets the total number of exercises assigned to a specific workout day.
    /// </summary>
    /// <param name="workoutDayId">The unique identifier of the workout day.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the exercise count for that day.
    /// </returns>
    /// <remarks>
    /// Lightweight check without loading the actual exercise data.
    /// Useful for validation (e.g., ensuring a workout day has at least one exercise before activation).
    /// </remarks>
    Task<int> GetTotalExercisesCountAsync(int workoutDayId);
}