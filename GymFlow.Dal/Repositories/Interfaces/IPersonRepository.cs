using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Repositories.Interfaces;

/// <summary>
/// Repository interface for Person entity
/// </summary>
public interface IPersonRepository : IRepository<Person>
{
    /// <summary>
    /// Gets a person with their associated User or Coach data
    /// </summary>
    Task<Person?> GetPersonWithRoleAsync(int personId);
    
    /// <summary>
    /// Gets a person by username
    /// </summary>
    Task<Person?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Gets a person with their User data (workouts, progress)
    /// </summary>
    Task<Person?> GetPersonWithUserDetailsAsync(int personId);
    
    /// <summary>
    /// Gets a person with their Coach data
    /// </summary>
    Task<Person?> GetPersonWithCoachDetailsAsync(int personId);
    
    /// <summary>
    /// Authenticates a person by username and password
    /// </summary>
    Task<Person?> AuthenticateAsync(string username, string password);
}