namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Repository interface for User-specific operations extending the generic repository.
/// </summary>
/// <remarks>
/// Provides specialized query methods for user data, including eager loading of related entities
/// and domain-specific business logic queries.
/// </remarks>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Retrieves a user with all their workout plans included.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the user with their workout plans populated, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// This method uses eager loading to include all workout plans associated with the user,
    /// avoiding the N+1 query problem when accessing plans after fetching the user.
    /// </remarks>
    /// <example>
    /// <code>
    /// var user = await userRepository.GetUserWithWorkoutPlansAsync(5);
    /// foreach (var plan in user.WorkoutPlans) {
    ///     Console.WriteLine($"Plan: {plan.Phase}, Active: {plan.IsActive}");
    /// }
    /// </code>
    /// </example>
    Task<User?> GetUserWithWorkoutPlansAsync(int userId);
    
    /// <summary>
    /// Retrieves a user with all their progress logs included.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the user with their progress logs populated, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// Useful for analyzing user's weight trends and body composition changes over time.
    /// The logs are typically ordered by date for chronological analysis.
    /// </remarks>
    Task<User?> GetUserWithProgressLogsAsync(int userId);
    
    /// <summary>
    /// Retrieves a user with complete history including all workout plans and progress logs.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the user with all related data populated, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// This method loads:
    /// <list type="bullet">
    /// <item><description>All workout plans for the user</description></item>
    /// <item><description>All workout days within those plans (with exercises)</description></item>
    /// <item><description>All progress logs (weight, body fat, etc.)</description></item>
    /// </list>
    /// Use this for comprehensive user profiles or when generating detailed reports.
    /// </remarks>
    Task<User?> GetUserWithCompleteHistoryAsync(int userId);
    
    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the user with matching email, or <c>null</c> if not found.
    /// </returns>
    /// <remarks>
    /// Email addresses are unique in the system (enforced by database unique index).
    /// This method is commonly used for authentication and duplicate email checking.
    /// </remarks>
    Task<User?> GetUserByEmailAsync(string email);
    
    /// <summary>
    /// Retrieves all users who currently have an active workout plan.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of users with at least one active workout plan.
    /// </returns>
    /// <remarks>
    /// An "active user" is defined as someone who has at least one workout plan with IsActive = true.
    /// This is useful for reporting active gym members or sending notifications.
    /// </remarks>
    Task<IEnumerable<User>> GetActiveUsersAsync();
    
    /// <summary>
    /// Retrieves users filtered by their body type.
    /// </summary>
    /// <param name="bodyType">The body type to filter by (e.g., "Fit", "LeanMuscular", "Obese").</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains a collection of users matching the specified body type.
    /// </returns>
    /// <remarks>
    /// Body types are defined in the <see cref="Enums.BodyType"/> enum.
    /// This method helps coaches identify users with similar body compositions for group planning.
    /// </remarks>
    Task<IEnumerable<User>> GetUsersByBodyTypeAsync(string bodyType);
    
    /// <summary>
    /// Checks whether a user has any active workout plans.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains <c>true</c> if the user has any active workout plan; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This is a lightweight existence check that doesn't load the actual plan data.
    /// Useful for business logic that requires users to have an active plan before certain operations.
    /// </remarks>
    Task<bool> IsUserActiveInWorkoutAsync(int userId);
    
    /// <summary>
    /// Gets the total number of workout sessions completed by a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the total count of completed workout sessions.
    /// </returns>
    /// <remarks>
    /// This counts only actual completed sessions (WorkoutSession entities), not planned days.
    /// Useful for gamification (milestones, badges) or attendance tracking.
    /// </remarks>
    Task<int> GetTotalWorkoutCountAsync(int userId);
}