using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;

namespace GymFlow.Web.Pages;

public class LoginModel : PageModel
{
    private readonly ApiClient _apiClient;

    public LoginModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "لطفاً نام کاربری و رمز عبور را وارد کنید";
            return Page();
        }

        var (success, userId) = await _apiClient.LoginAsync(Username, Password);

        // لاگ دیباگ
        Console.WriteLine($"Login attempt: {Username}, success={success}, userId={userId}");

        if (success)
        {
            HttpContext.Session.SetString("Username", Username);
            HttpContext.Session.SetString("UserRole", Username == "coach" ? "Coach" : "Member");
            HttpContext.Session.SetString("UserId", userId.ToString());

            Console.WriteLine($"[DEBUG] UserId saved in session: {userId}");

            return RedirectToPage("/Index");
        }

        ErrorMessage = "نام کاربری یا رمز عبور اشتباه است";
        return Page();
    }
}