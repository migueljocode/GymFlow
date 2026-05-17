namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Repository interface for Coach-specific operations
/// </summary>
public interface ICoachRepository : IRepository<Coach>
{
    /// <summary>
    /// Gets a coach with their associated Person data
    /// </summary>
    Task<Coach?> GetCoachWithPersonAsync(int coachId);
    
    /// <summary>
    /// Gets a coach by PersonId
    /// </summary>
    Task<Coach?> GetByPersonIdAsync(int personId);
    
    /// <summary>
    /// Gets a coach by username (through Person)
    /// </summary>
    Task<Coach?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Gets all coaches with their Person data
    /// </summary>
    Task<IEnumerable<Coach>> GetAllCoachesWithPersonAsync();

    /// <summary>
    /// Gets a coach by User ID (through Person relation)
    /// </summary>
    Task<Coach?> GetByUserIdAsync(int userId);
}