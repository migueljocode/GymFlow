namespace GymFlow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthService(this IServiceCollection services)
    {
        services.AddScoped<IAuthService>(provider =>
        {
            var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            return new AuthService(factory);
        });
        return services;
    }

    public static IServiceCollection AddBasicSeeder(this IServiceCollection services)
    {
        services.AddScoped<DatabaseBasicSeeder>();
        return services;
    }
}