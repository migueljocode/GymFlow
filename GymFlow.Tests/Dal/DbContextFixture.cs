namespace GymFlow.Tests.Dal;

/// <summary>
/// Fixture for providing a fresh in-memory database for each test
/// </summary>
public class DbContextFixture : IDisposable
{
    private readonly string _dbName;
    public AppDbContextFactory DbContextFactory { get; private set; }

    public DbContextFixture()
    {
        _dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"DataSource=file:{_dbName}.db?mode=memory&cache=shared")
            .Options;

        // استفاده از AppDbContextFactory موجود
        DbContextFactory = new AppDbContextFactory(options);
        
        using var context = DbContextFactory.CreateDbContext();
        context.Database.Migrate();
    }

    /// <summary>
    /// Create a new isolated context for testing
    /// </summary>
    public AppDbContext CreateContext()
    {
        return DbContextFactory.CreateDbContext();
    }

    /// <summary>
    /// Seed database with Bogus test data
    /// </summary>
    public async Task SeedWithBogusDataAsync()
    {
        await using var context = CreateContext();
        
        var options = SeedProfiles.Lightweight;
        var generator = new SeedDataGenerator(options);
        
        // 1. Exercises
        var exercises = generator.GenerateExercises();
        await context.Exercises.AddRangeAsync(exercises);
        await context.SaveChangesAsync();
        
        // 2. Persons
        var persons = generator.GeneratePersons();
        await context.Persons.AddRangeAsync(persons);
        await context.SaveChangesAsync();
        
        // 3. Users
        var users = generator.GenerateUsers(persons);
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        
        // 4. Coaches
        var coaches = generator.GenerateCoaches(persons);
        await context.Coaches.AddRangeAsync(coaches);
        await context.SaveChangesAsync();
        
        // 5. WorkoutData
        var (workoutPlans, workoutDays, workoutDayExercises, progressLogs, workoutSessions) = 
            generator.GenerateWorkoutData(users, exercises);
        
        await context.WorkoutPlans.AddRangeAsync(workoutPlans);
        await context.SaveChangesAsync();
        
        await context.WorkoutDays.AddRangeAsync(workoutDays);
        await context.SaveChangesAsync();
        
        await context.WorkoutDayExercises.AddRangeAsync(workoutDayExercises);
        await context.SaveChangesAsync();
        
        await context.ProgressLogs.AddRangeAsync(progressLogs);
        await context.SaveChangesAsync();
        
        await context.WorkoutSessions.AddRangeAsync(workoutSessions);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Reset database (delete and recreate)
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }
}