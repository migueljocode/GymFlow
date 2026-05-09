namespace GymFlow.Dal.Seed.Extensions;

/// <summary>
/// Extension methods for automatic database seeding and migration.
/// </summary>
public static class DatabaseSeedExtensions
{
    /// <summary>
    /// Automatically migrates and seeds the database (call in Program.cs).
    /// </summary>
    public static async Task EnsureDatabaseSeededAsync(this IServiceProvider serviceProvider, SeedOptions? options = null)
    {
        using var scope = serviceProvider.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        
        var seedOptions = options ?? scope.ServiceProvider.GetService<SeedOptions>() ?? SeedProfiles.Development;
        
        var seeder = new DatabaseSeeder(contextFactory, seedOptions);
        await seeder.SeedAsync();
    }
    
    /// <summary>
    /// Reseeds the database completely (clear + generate fresh data).
    /// </summary>
    public static async Task ReseedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        var freshOptions = SeedProfiles.Development;
        freshOptions.ClearExistingData = true;
        
        using var scope = serviceProvider.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        
        var seeder = new DatabaseSeeder(contextFactory, freshOptions);
        await seeder.SeedAsync();
    }
}