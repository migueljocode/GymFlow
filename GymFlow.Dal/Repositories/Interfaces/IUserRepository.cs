namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Repository interface for User-specific operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user with their associated Person data
    /// </summary>
    Task<User?> GetUserWithPersonAsync(int userId);
    
    /// <summary>
    /// Gets a user with their workout plans
    /// </summary>
    Task<User?> GetUserWithWorkoutPlansAsync(int userId);
    
    /// <summary>
    /// Gets a user with their progress logs
    /// </summary>
    Task<User?> GetUserWithProgressLogsAsync(int userId);
    
    /// <summary>
    /// Gets a user by PersonId
    /// </summary>
    Task<User?> GetByPersonIdAsync(int personId);
    
    /// <summary>
    /// Gets a user by username (through Person)
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Gets all users with their Person data
    /// </summary>
    Task<IEnumerable<User>> GetAllUsersWithPersonAsync();

    Task<User?> GetUserWithCompleteHistoryAsync(int userId);
    Task<User?> GetUserByEmailAsync(string email);
}