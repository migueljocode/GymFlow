namespace GymFlow.Dal.Seed.Data;

/// <summary>
/// Main database seeder that orchestrates the complete seeding process with refresh capabilities.
/// </summary>
public class DatabaseSeeder
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly SeedOptions _options;
    
    public DatabaseSeeder(IDbContextFactory<AppDbContext> dbContextFactory, SeedOptions? options = null)
    {
        _dbContextFactory = dbContextFactory;
        _options = options ?? SeedProfiles.Development;  // ← Changed from SeedOptions.Development
    }
    
    /// <summary>
    /// Seeds the database with rich test data. In development mode, this refreshes all data.
    /// </summary>
    public async Task SeedAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        
        // Check if database exists and has tables
        var databaseExists = await context.Database.CanConnectAsync();
        
        if (!databaseExists)
        {
            Console.WriteLine("📦 Creating database and applying migrations...");
            await context.Database.MigrateAsync();
        }
        
        // Determine if we should seed
        var shouldSeed = ShouldSeed(context);
        
        if (!shouldSeed)
        {
            Console.WriteLine("⏭️ Skipping seeding - conditions not met");
            return;
        }
        
        // Clear existing data if requested
        if (_options.ClearExistingData && await HasDataAsync(context))
        {
            await ClearDatabaseAsync(context);
        }
        
        // Generate and seed data
        Console.WriteLine("🌱 Seeding database with rich test data...");
        Console.WriteLine("===============================================");
        
        var generator = new SeedDataGenerator(_options);
        var data = generator.GenerateAllData();
        
        // Save in correct order (respecting foreign keys)
        await context.Exercises.AddRangeAsync(data.Exercises);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Added {data.Exercises.Count} exercises");
        
        await context.Users.AddRangeAsync(data.Users);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Added {data.Users.Count} users");
        
        await context.WorkoutPlans.AddRangeAsync(data.WorkoutPlans);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Added {data.WorkoutPlans.Count} workout plans");
        
        await context.WorkoutDays.AddRangeAsync(data.WorkoutDays);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Added {data.WorkoutDays.Count} workout days");
        
        await context.WorkoutDayExercises.AddRangeAsync(data.WorkoutDayExercises);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Added {data.WorkoutDayExercises.Count} workout day exercises");
        
        await context.ProgressLogs.AddRangeAsync(data.ProgressLogs);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Added {data.ProgressLogs.Count} progress logs");
        
        await context.WorkoutSessions.AddRangeAsync(data.WorkoutSessions);
        await context.SaveChangesAsync();
        
        Console.WriteLine("===============================================");
        Console.WriteLine("✅ DATABASE SEEDING COMPLETED SUCCESSFULLY!");
        Console.WriteLine($"📊 Total records: {GetTotalRecordCount(data)}");
    }
    
    /// <summary>
    /// Determines whether seeding should run based on options and current state.
    /// </summary>
    private bool ShouldSeed(AppDbContext context)
    {
        if (_options.RefreshOnStartup)
        {
            Console.WriteLine("🔄 Refresh mode: ON - Will reseed database");
            return true;
        }
        
        if (_options.SeedOnlyIfEmpty && !HasDataAsync(context).GetAwaiter().GetResult())
        {
            Console.WriteLine("📭 Database is empty - Seeding...");
            return true;
        }
        
        if (!_options.SeedOnlyIfEmpty)
        {
            Console.WriteLine("📝 Seeding enabled regardless of existing data");
            return true;
        }
        
        Console.WriteLine("⏭️ Seeding skipped (database not empty and SeedOnlyIfEmpty=true)");
        return false;
    }
    
    /// <summary>
    /// Checks if the database already has any data.
    /// </summary>
    private async Task<bool> HasDataAsync(AppDbContext context)
    {
        return await context.Users.AnyAsync() || await context.Exercises.AnyAsync();
    }
    
    /// <summary>
    /// Clears all data from the database.
    /// </summary>
    private async Task ClearDatabaseAsync(AppDbContext context)
    {
        Console.WriteLine("🗑️ Clearing existing database data...");
        
        // Order matters for foreign keys
        await context.WorkoutSessions.ExecuteDeleteAsync();
        await context.WorkoutDayExercises.ExecuteDeleteAsync();
        await context.ProgressLogs.ExecuteDeleteAsync();
        await context.WorkoutDays.ExecuteDeleteAsync();
        await context.WorkoutPlans.ExecuteDeleteAsync();
        await context.Exercises.ExecuteDeleteAsync();
        await context.Users.ExecuteDeleteAsync();
        
        // Reset SQLite auto-increment counters
        await context.Database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence;");
        
        Console.WriteLine("  ✅ Database cleared");
    }
    
    private int GetTotalRecordCount(SeedDataResult data) =>
        data.Exercises.Count + data.Users.Count + data.WorkoutPlans.Count +
        data.WorkoutDays.Count + data.WorkoutDayExercises.Count +
        data.ProgressLogs.Count + data.WorkoutSessions.Count;
}