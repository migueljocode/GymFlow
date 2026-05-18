namespace GymFlow.Dal.Repositories.Implementations;

public class CoachRepository : Repository<Coach>, ICoachRepository
{
    public CoachRepository(IDbContextFactory<AppDbContext> dbContextFactory) 
        : base(dbContextFactory) { }

    public async Task<Coach?> GetCoachWithPersonAsync(int coachId)
    {
        await using var context = await CreateContextAsync();
        return await context.Coaches
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.Id == coachId);
    }

    public async Task<Coach?> GetByPersonIdAsync(int personId)
    {
        await using var context = await CreateContextAsync();
        return await context.Coaches
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.PersonId == personId);
    }

    public async Task<Coach?> GetByUsernameAsync(string username)
    {
        await using var context = await CreateContextAsync();
        return await context.Coaches
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.Person != null && c.Person.Username == username);
    }

    public async Task<IEnumerable<Coach>> GetAllCoachesWithPersonAsync()
    {
        await using var context = await CreateContextAsync();
        return await context.Coaches
            .Include(c => c.Person)
            .ToListAsync();
    }

    public async Task<Coach?> GetByUserIdAsync(int userId)
    {
        await using var context = await CreateContextAsync();
        
        // ابتدا Person را از طریق User پیدا می‌کنیم
        var user = await context.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user?.Person == null) 
            return null;
        
        // سپس Coach را از طریق PersonId پیدا می‌کنیم
        return await context.Coaches
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.PersonId == user.Person.Id);
    }
}