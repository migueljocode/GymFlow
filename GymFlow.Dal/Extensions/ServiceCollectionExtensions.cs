namespace GymFlow.Dal.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers DbContext and DbContextFactory
    /// </summary>
    public static IServiceCollection AddGymFlowDbContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));
       
        services.AddScoped<IDbContextFactory<AppDbContext>>(provider =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite(connectionString);
            return new AppDbContextFactory(optionsBuilder.Options);
        });
        return services;
    }
    
    /// <summary>
    /// Registers all repositories
    /// </summary>
    public static IServiceCollection AddGymFlowRepositories(this IServiceCollection services)
    {
        // Register Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICoachRepository, CoachRepository>();
        services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();
        services.AddScoped<IWorkoutDayRepository, WorkoutDayRepository>();
        services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
        services.AddScoped<IProgressLogRepository, ProgressLogRepository>();
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        
        return services;
    }
    
    /// <summary>
    /// Gets connection string from configuration
    /// </summary>
    public static string GetGymFlowConnectionString(
        this IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
        
        return connectionString;
    }
    
    /// <summary>
    /// Configures seed options based on hosting environment
    /// </summary>
    public static IServiceCollection ConfigureSeedForEnvironment(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.UseDevelopmentSeed();
        }
        else
        {
            services.UseLightweightSeed();
        }
        
        return services;
    }
    
    /// <summary>
    /// Complete DAL setup (composes all of the above)
    /// </summary>
    public static IServiceCollection AddGymFlowDal(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string connectionStringName = "DefaultConnection")
    {
        var connectionString = configuration.GetGymFlowConnectionString(connectionStringName);
        
        services.AddGymFlowDbContext(connectionString);
        services.AddGymFlowRepositories();
        services.ConfigureSeedForEnvironment(environment);
        
        return services;
    }
}