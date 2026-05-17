using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;
using System.Text.Json;

namespace GymFlow.Web.Pages;

public class ProfileModel : BasePageModel
{
    private readonly ApiClient _apiClient;

    public ProfileModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? Phone { get; set; }

    [BindProperty]
    public string Specialization { get; set; } = string.Empty;

    [BindProperty]
    public int YearsOfExperience { get; set; }

    public string Username { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfNotCoach();
        if (redirect != null) return redirect;

        await LoadProfileAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var redirect = RedirectIfNotCoach();
        if (redirect != null) return redirect;

        if (!ModelState.IsValid)
        {
            ErrorMessage = "لطفاً اطلاعات را به درستی وارد کنید.";
            return Page();
        }

        var updateData = new
        {
            FirstName,
            LastName,
            Email,
            Phone,
            Specialization,
            YearsOfExperience
        };

        var success = await _apiClient.PutAsync("api/coaches/me", updateData);
        if (success)
        {
            Message = "اطلاعات پروفایل با موفقیت به‌روز شد.";
            // بارگذاری مجدد اطلاعات (برای نمایش مقادیر جدید)
            await LoadProfileAsync();
        }
        else
        {
            ErrorMessage = "خطا در به‌روزرسانی پروفایل. لطفاً دوباره تلاش کنید.";
        }

        return Page();
    }

    private async Task LoadProfileAsync()
    {
        var profile = await _apiClient.GetAsync<CoachProfileDto>("api/coaches/me");
        if (profile != null)
        {
            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Email = profile.Email;
            Phone = profile.Phone;
            Specialization = profile.Specialization;
            YearsOfExperience = profile.YearsOfExperience;
            Username = profile.Username;
            CreatedAt = profile.CreatedAt.ToString("yyyy/MM/dd");
        }
        else
        {
            ErrorMessage = "خطا در دریافت اطلاعات پروفایل.";
        }
    }
}

public class CoachProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}