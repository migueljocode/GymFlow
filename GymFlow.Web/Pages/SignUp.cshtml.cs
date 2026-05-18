using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;

namespace GymFlow.Web.Pages;

public class SignUpModel : PageModel
{
    private readonly ApiClient _apiClient;

    public SignUpModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string Role { get; set; } = "Member";

    public async Task<IActionResult> OnPostAsync()
    {
        // اعتبارسنجی ساده
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            TempData["ErrorMessage"] = "نام کاربری و رمز عبور الزامی هستند.";
            return Page();
        }

        if (Password.Length < 6)
        {
            TempData["ErrorMessage"] = "رمز عبور باید حداقل ۶ کاراکتر باشد.";
            return Page();
        }

        if (Password != ConfirmPassword)
        {
            TempData["ErrorMessage"] = "رمز عبور و تکرار آن مطابقت ندارند.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            TempData["ErrorMessage"] = "نام و نام خانوادگی الزامی هستند.";
            return Page();
        }

        // ارسال درخواست ثبت‌نام به API
        var request = new
        {
            Username = Username,
            Password = Password,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email ?? "",
            Role = Role
        };

        var (result, errorMessage) = await _apiClient.PostWithErrorAsync<object>("api/auth/register", request);

        // دیباگ - خروجی را در کنسول ببینید
        Console.WriteLine($"Register result: {result}, ErrorMessage: {errorMessage}");

        // اگر خطایی وجود نداشت (یعنی ثبت‌نام موفق بود)
        if (string.IsNullOrEmpty(errorMessage))
        {
            TempData["Message"] = $"✅ ثبت‌نام با موفقیت انجام شد! اکنون می‌توانید وارد شوید.";
            return RedirectToPage("/Login");
        }

        TempData["ErrorMessage"] = errorMessage;
        return Page();
    }
}