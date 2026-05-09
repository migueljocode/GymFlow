namespace GymFlow.Dal.Seed.Extensions;

public static class ServiceCollectionSeedExtensions
{
    /// <summary>
    /// Configures seed options for the application.
    /// </summary>
    public static IServiceCollection ConfigureSeedOptions(this IServiceCollection services, SeedOptions options)
    {
        services.AddSingleton(options);
        return services;
    }
    
    /// <summary>
    /// Uses development seed preset.
    /// </summary>
    public static IServiceCollection UseDevelopmentSeed(this IServiceCollection services) =>
        services.ConfigureSeedOptions(SeedProfiles.Development);
    
    /// <summary>
    /// Uses quick demo seed preset.
    /// </summary>
    public static IServiceCollection UseQuickDemoSeed(this IServiceCollection services) =>
        services.ConfigureSeedOptions(SeedProfiles.QuickDemo);
    
    /// <summary>
    /// Uses lightweight seed preset.
    /// </summary>
    public static IServiceCollection UseLightweightSeed(this IServiceCollection services) =>
        services.ConfigureSeedOptions(SeedProfiles.Lightweight);
    
    /// <summary>
    /// Uses stress test seed preset (large data volume).
    /// </summary>
    public static IServiceCollection UseStressTestSeed(this IServiceCollection services) =>
        services.ConfigureSeedOptions(SeedProfiles.StressTest);
    
    /// <summary>
    /// Uses production seed preset (minimal, no auto-refresh).
    /// </summary>
    public static IServiceCollection UseProductionSeed(this IServiceCollection services) =>
        services.ConfigureSeedOptions(SeedProfiles.Production);
    
    /// <summary>
    /// Uses empty seed preset (clears database, adds no data).
    /// </summary>
    public static IServiceCollection UseEmptySeed(this IServiceCollection services) =>
        services.ConfigureSeedOptions(SeedProfiles.Empty);
    
    /// <summary>
    /// Uses a seed profile by name.
    /// </summary>
    public static IServiceCollection UseSeedProfile(this IServiceCollection services, string profileName) =>
        services.ConfigureSeedOptions(SeedProfiles.GetByName(profileName));
}