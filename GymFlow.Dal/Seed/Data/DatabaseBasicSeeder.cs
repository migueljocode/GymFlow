using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Dal.Seed.Data;

/// <summary>
/// Seeder مستقل که فقط کاربران پایه (coach و member) را تضمین می‌کند.
/// </summary>
public class DatabaseBasicSeeder
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public DatabaseBasicSeeder(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task EnsureBasicUsersAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        
        // 🔧 ایجاد دیتابیس و جداول در صورت عدم وجود
        await context.Database.MigrateAsync();

        // 1. ایجاد Coach Person
        var coachPerson = await context.Persons
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Username == "coach");

        if (coachPerson == null)
        {
            coachPerson = new Person
            {
                Username = "coach",
                Password = "coach123",
                FirstName = "Master",
                LastName = "Coach",
                Email = "coach@gymflow.com",
                Phone = "+1 (555) 000-0001",
                Gender = Gender.Male,
                Age = 35,
                Weight = 85.5f,
                Height = 182f,
                BodyType = BodyType.Fit,
                CreatedAt = DateTime.UtcNow
            };
            context.Persons.Add(coachPerson);
            await context.SaveChangesAsync();
        }

        // Coach (رابطه یک به یک با Person)
        if (coachPerson.User == null)
        {
            var coachUser = new User
            {
                PersonId = coachPerson.Id,
                Goal = Goal.Fitness,
                CreatedAt = coachPerson.CreatedAt
            };
            context.Users.Add(coachUser);
            await context.SaveChangesAsync();
        }

        // Coach (موجودیت Coach)
        var coachEntity = await context.Coaches
            .FirstOrDefaultAsync(c => c.PersonId == coachPerson.Id);
        if (coachEntity == null)
        {
            coachEntity = new Coach
            {
                PersonId = coachPerson.Id,
                Specialization = "Strength & Conditioning",
                YearsOfExperience = 10,
                CertificateUrl = "https://example.com/certificate.pdf",
                CreatedAt = coachPerson.CreatedAt
            };
            context.Coaches.Add(coachEntity);
            await context.SaveChangesAsync();
        }

        // 2. ایجاد Member Person
        var memberPerson = await context.Persons
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Username == "member");

        if (memberPerson == null)
        {
            memberPerson = new Person
            {
                Username = "member",
                Password = "member123",
                FirstName = "John",
                LastName = "Doe",
                Email = "member@gymflow.com",
                Phone = "+1 (555) 000-0002",
                Gender = Gender.Male,
                Age = 25,
                Weight = 75.0f,
                Height = 175f,
                BodyType = BodyType.Fit,
                CreatedAt = DateTime.UtcNow
            };
            context.Persons.Add(memberPerson);
            await context.SaveChangesAsync();
        }

        if (memberPerson.User == null)
        {
            var memberUser = new User
            {
                PersonId = memberPerson.Id,
                Goal = Goal.MuscleGain,
                EstimatedCaloriesIntake = 2500,
                IsCompetitive = false,
                CreatedAt = memberPerson.CreatedAt
            };
            context.Users.Add(memberUser);
            await context.SaveChangesAsync();
        }

        // در صورت لزوم، می‌توانید coach را به member اختصاص دهید (اختیاری)
        var memberUserEntity = await context.Users
            .FirstOrDefaultAsync(u => u.PersonId == memberPerson.Id);
        if (memberUserEntity != null && memberUserEntity.CoachId == null && coachEntity != null)
        {
            memberUserEntity.CoachId = coachEntity.Id;
            await context.SaveChangesAsync();
        }

        Console.WriteLine("✅ Basic users (coach & member) ensured.");
    }
}