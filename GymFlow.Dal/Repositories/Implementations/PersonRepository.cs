using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Repositories.Implementations;

/// <summary>
/// Repository implementation for Person entity
/// </summary>
public class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(IDbContextFactory<AppDbContext> dbContextFactory) 
        : base(dbContextFactory) { }

    public async Task<Person?> GetPersonWithRoleAsync(int personId)
    {
        await using var context = await CreateContextAsync();
        return await context.Persons
            .Include(p => p.User)
            .Include(p => p.Coach)
            .FirstOrDefaultAsync(p => p.Id == personId);
    }

    public async Task<Person?> GetByUsernameAsync(string username)
    {
        await using var context = await CreateContextAsync();
        return await context.Persons
            .FirstOrDefaultAsync(p => p.Username == username);
    }

    public async Task<Person?> GetPersonWithUserDetailsAsync(int personId)
    {
        await using var context = await CreateContextAsync();
        return await context.Persons
            .Include(p => p.User!)
                .ThenInclude(u => u.WorkoutPlans)
            .Include(p => p.User!)
                .ThenInclude(u => u.ProgressLogs)
            .FirstOrDefaultAsync(p => p.Id == personId);
    }

    public async Task<Person?> GetPersonWithCoachDetailsAsync(int personId)
    {
        await using var context = await CreateContextAsync();
        return await context.Persons
            .Include(p => p.Coach)
            .FirstOrDefaultAsync(p => p.Id == personId);
    }

    public async Task<Person?> AuthenticateAsync(string username, string password)
    {
        await using var context = await CreateContextAsync();
        return await context.Persons
            .FirstOrDefaultAsync(p => p.Username == username && p.Password == password);
    }
}