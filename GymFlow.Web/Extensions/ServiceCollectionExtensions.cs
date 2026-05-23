namespace GymFlow.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomSession(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        return services;
    }
}