namespace GymFlow.Dal.Factories;

/// <summary>
/// Design-time factory for migrations (used by dotnet ef)
/// </summary>
public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=../GymFlow.db");
        return new AppDbContext(optionsBuilder.Options);
    }
}