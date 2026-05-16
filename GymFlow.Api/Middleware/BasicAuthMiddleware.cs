using System.Text;
using GymFlow.Services.Interfaces;

namespace GymFlow.Api.Middleware;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;

    public BasicAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        // فقط درخواست‌هایی که نیاز به احراز هویت دارند بررسی شوند
        var protectedPaths = new[] { "/api/workoutplans", "/api/workoutdays", "/api/workoutsessions", "/api/progress", "/api/statistics", "/api/predictions" };
        
        // تغییر: مسیر درخواست را به حروف کوچک تبدیل می‌کنیم تا مقایسه case-insensitive باشد
        var requestPath = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var isProtected = protectedPaths.Any(p => requestPath.StartsWith(p));
        
        if (!isProtected)
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].ToString();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
        {
            context.Response.StatusCode = 401;
            context.Response.Headers["WWW-Authenticate"] = "Basic";
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        try
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var separatorIndex = decodedCredentials.IndexOf(':');
            
            if (separatorIndex == -1)
                throw new FormatException();

            var username = decodedCredentials.Substring(0, separatorIndex);
            var password = decodedCredentials.Substring(separatorIndex + 1);

            var user = await authService.AuthenticateAsync(username, password);
            
            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid username or password");
                return;
            }

            context.Items["User"] = user;
            context.Items["UserId"] = user.Id;
            context.Items["Username"] = user.Person?.Username ?? username;
            context.Items["UserRole"] = username == "coach" ? "Coach" : "Member";
        }
        catch (Exception)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid authorization header");
            return;
        }

        await _next(context);
    }
}