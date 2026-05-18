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
        
        var person = await context.Persons.FirstAsync(x => x.Username == username);
 
        if (person is null)
            return null;

        var isPasswordTrue = person.Password == password;

        return isPasswordTrue ? person : null; 
    }

    public override async Task<Person> AddAsync(Person entity)
    {
        await using var context = await CreateContextAsync();
        await context.Persons.AddAsync(entity);
        try
        {
            await context.SaveChangesAsync();        
            return entity;
        }
        catch (DbUpdateException ex)
        {
            string message = "";
            // بررسی خطای یکتایی (Unique Constraint)
            if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true ||
                ex.Message.Contains("IX_Persons_Username") == true)
            {
                message = "این نام کاربری قبلاً ثبت شده است. لطفاً نام کاربری دیگری انتخاب کنید.";
            }
            else
                message = ex.Message;

            throw new Exception(message);
        }
    }
}