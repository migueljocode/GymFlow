namespace GymFlow.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task EnsureBasicUsersAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var basicSeeder = scope.ServiceProvider.GetRequiredService<DatabaseBasicSeeder>();
        await basicSeeder.EnsureBasicUsersAsync();
    }

    public static async Task SeedDevelopmentDataAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            await app.Services.EnsureDatabaseSeededAsync();
        }
    }
}