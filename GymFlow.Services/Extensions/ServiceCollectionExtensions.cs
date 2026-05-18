namespace GymFlow.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGymFlowServices(this IServiceCollection services)
    {
        services.AddScoped<IWeightPredictionService, WeightPredictionService>();
        services.AddScoped<IPdfExportService, PdfExportService>();
        services.AddScoped<IWorkoutAnalyticsService, WorkoutAnalyticsService>();
        services.AddScoped<IUserDashboardService, UserDashboardService>();
        
        return services;
    }
}