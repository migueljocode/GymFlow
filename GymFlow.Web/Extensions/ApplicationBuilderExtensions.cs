namespace GymFlow.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseCustomErrorHandling(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }
        return app;
    }
}