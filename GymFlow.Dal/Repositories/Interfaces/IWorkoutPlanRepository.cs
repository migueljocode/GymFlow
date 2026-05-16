using GymFlow.Models.Entities;

namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Repository interface for WorkoutPlan-specific operations extending the generic repository.
/// </summary>
/// <remarks>
/// Provides specialized query methods for workout plan management, including detailed plan loading,
/// activation/deactivation, and phase-based queries.
/// </remarks>
public interface IWorkoutPlanRepository : IRepository<WorkoutPlan>
{
    /// <summary>
    /// Retrieves a workout plan with all its details including days, exercises, and progress logs.
    /// </summary>
    /// <param name="planId">The unique identifier of the workout plan.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the plan with fully populated navigation properties, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// This method eagerly loads:
    /// <list type="bullet">
    /// <item><description>WorkoutDays with their DayOfWeek and parameters</description></item>
    /// <item><description>WorkoutDayExercises (sets, reps, rest times)</description></item>
    /// <item><description>Exercise library details (name, muscle group)</description></item>
    /// <item><description>ProgressLogs associated with this plan</description></item>
    /// </list>
    /// Use this for complete plan visualization in the UI.
    /// </remarks>
    Task<WorkoutPlan?> GetWorkoutPlanWithDetailsAsync(int planId);
    
    /// <summary>
    /// Retrieves a workout plan with only its workout days (no exercises or progress logs).
    /// </summary>
    /// <param name="planId">The unique identifier of the workout plan.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the plan with WorkoutDays populated, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// Lighter than <see cref="GetWorkoutPlanWithDetailsAsync"/> when exercise details aren't needed.
    /// Useful for plan summary views or calendar displays.
    /// </remarks>
    Task<WorkoutPlan?> GetWorkoutPlanWithDaysAsync(int planId);
    
    /// <summary>
    /// Retrieves a workout plan with its associated progress logs.
    /// </summary>
    /// <param name="planId">The unique identifier of the workout plan.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the plan with ProgressLogs populated, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// Use this to analyze weight trends and performance improvements during a specific plan period.
    /// </remarks>
    Task<WorkoutPlan?> GetWorkoutPlanWithProgressAsync(int planId);
    
    /// <summary>
    /// Retrieves all workout plans for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of workout plans belonging to the user, ordered by start date (newest first).
    /// </returns>
    /// <remarks>
    /// This returns plans without loading child entities for performance.
    /// Use <see cref="GetWorkoutPlanWithDetailsAsync"/> for detailed views of individual plans.
    /// </remarks>
    Task<IEnumerable<WorkoutPlan>> GetUserWorkoutPlansAsync(int userId);
    
    /// <summary>
    /// Retrieves the currently active workout plan for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the active plan, or <c>null</c> if no active plan exists.
    /// </returns>
    /// <remarks>
    /// A user should ideally have only one active plan at a time. This method returns the plan where <c>IsActive = true</c>.
    /// If multiple are found (data inconsistency), returns the first one.
    /// </remarks>
    Task<WorkoutPlan?> GetActiveWorkoutPlanAsync(int userId);
    
    /// <summary>
    /// Retrieves the current workout plan based on date range (not just IsActive flag).
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the plan applicable for today's date, or <c>null</c> if none.
    /// </returns>
    /// <remarks>
    /// A plan is considered "current" if:
    /// <list type="bullet">
    /// <item><description>StartDate ≤ today's date</description></item>
    /// <item><description>EndDate is null OR EndDate ≥ today's date</description></item>
    /// </list>
    /// This is more accurate than IsActive alone when plans have specific date ranges.
    /// </remarks>
    Task<WorkoutPlan?> GetCurrentWorkoutPlanAsync(int userId);
    
    /// <summary>
    /// Retrieves workout plans for a specific phase number.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="phase">The phase number (1, 2, 3, etc. - typically increments every 2 months).</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of plans matching the phase number.
    /// </returns>
    /// <remarks>
    /// Gym programs typically progress through phases. This helps track historical plan versions.
    /// </remarks>
    Task<IEnumerable<WorkoutPlan>> GetWorkoutPlansByPhaseAsync(int userId, int phase);
    
    /// <summary>
    /// Deactivates all active workout plans for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if any plans were deactivated; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is typically called before activating a new plan to ensure only one plan is active at a time.
    /// Each affected plan has its <c>IsActive</c> set to false and <c>UpdatedAt</c> timestamp updated.
    /// </remarks>
    Task<bool> DeactivateAllUserPlansAsync(int userId);
    
    /// <summary>
    /// Activates a specific workout plan.
    /// </summary>
    /// <param name="planId">The unique identifier of the plan to activate.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if activation succeeded; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method doesn't automatically deactivate other plans. Call <see cref="DeactivateAllUserPlansAsync"/>
    /// first if you want a single active plan per user.
    /// </remarks>
    Task<bool> ActivateWorkoutPlanAsync(int planId);

    /// <summary>
    /// Deactivates all active plans for a user and creates a new plan in a single transaction.
    /// </summary>
    Task<WorkoutPlan> DeactivateAllAndAddAsync(WorkoutPlan newPlan);
}