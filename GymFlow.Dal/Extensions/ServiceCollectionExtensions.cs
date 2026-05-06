namespace GymFlow.Dal.Extensions;

/// <summary>
/// Extension methods for configuring GymFlow.Dal services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds GymFlow Dal services to the DI container
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configuration">IConfiguration</param>
    /// <param name="connectionStringName">Name of connection string in appsettings (default: "DefaultConnection")</param>
    /// <returns>IServiceCollection for chaining</returns>
    public static IServiceCollection AddGymFlowDal(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");
        }
        
        return services.AddGymFlowDal(connectionString);
    }
    
    /// <summary>
    /// Adds GymFlow Dal services to the DI container with explicit connection string
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>IServiceCollection for chaining</returns>
    public static IServiceCollection AddGymFlowDal(
        this IServiceCollection services,
        string connectionString)
    {
        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));
        
        // Register IDbContextFactory for runtime
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite(connectionString));
        
        // Register Runtime Factory as Singleton
        services.AddSingleton<IDbContextFactory<AppDbContext>>(provider =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite(connectionString);
            return new AppDbContextFactory(optionsBuilder.Options);
        });
        
        // Register Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Register Specific Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();
        services.AddScoped<IWorkoutDayRepository, WorkoutDayRepository>();
        services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
        services.AddScoped<IProgressLogRepository, ProgressLogRepository>();
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        
        return services;
    }
}