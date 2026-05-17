using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;
using GymFlow.Models.Enums;

namespace GymFlow.Web.Pages;

public class ProfileModel : BasePageModel
{
    private readonly ApiClient _apiClient;

    public ProfileModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // ========== فیلدهای مشترک ==========
    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? Phone { get; set; }

    // ========== فیلدهای مخصوص مربی ==========
    [BindProperty]
    public string Specialization { get; set; } = string.Empty;

    [BindProperty]
    public int YearsOfExperience { get; set; }

    // ========== فیلدهای مخصوص کاربر عادی ==========
    [BindProperty]
    public Goal Goal { get; set; }

    [BindProperty]
    public float? Weight { get; set; }

    [BindProperty]
    public float? Height { get; set; }

    [BindProperty]
    public BodyType? BodyType { get; set; }

    [BindProperty]
    public int? EstimatedCaloriesIntake { get; set; }

    [BindProperty]
    public bool IsCompetitive { get; set; }

    // ========== اطلاعات فقط خواندنی ==========
    public string Username { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;

    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // هر دو نقش باید لاگین باشند
        if (!IsMember && !IsCoach) return RedirectToPage("/Login");

        await LoadProfileAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!IsMember && !IsCoach) return RedirectToPage("/Login");

        // حذف خطاهای فیلدهای غیرمرتبط با نقش کاربر
        if (IsMember)
        {
            ModelState.Remove("Specialization");
            ModelState.Remove("YearsOfExperience");
        }
        if (IsCoach)
        {
            ModelState.Remove("Goal");
            ModelState.Remove("Weight");
            ModelState.Remove("Height");
            ModelState.Remove("BodyType");
            ModelState.Remove("EstimatedCaloriesIntake");
            ModelState.Remove("IsCompetitive");
        }


        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
    
            ErrorMessage = $"لطفاً اطلاعات را به درستی وارد کنید: {string.Join(" | ", errors)}";
    
            return Page();
        }

        bool success;
        if (IsCoach)
        {
            var updateData = new
            {
                FirstName,
                LastName,
                Email,
                Phone,
                Specialization,
                YearsOfExperience
            };
            success = await _apiClient.PutAsync("api/coaches/me", updateData);
        }
        else
        {
            var userId = GetCurrentUserIdFromSession();
            if (userId == null) return RedirectToPage("/Login");

            var updateData = new
            {
                FirstName,
                LastName,
                Email,
                Phone,
                Goal,
                Weight,
                Height,
                BodyType,
                EstimatedCaloriesIntake,
                IsCompetitive
            };
            success = await _apiClient.PutAsync($"api/users/{userId}", updateData);
        }

        if (success)
        {
            Message = "اطلاعات پروفایل با موفقیت به‌روز شد.";
            await LoadProfileAsync(); // بارگذاری مجدد مقادیر جدید
        }
        else
        {
            ErrorMessage = "خطا در به‌روزرسانی پروفایل. لطفاً دوباره تلاش کنید.";
        }

        return Page();
    }

    private async Task LoadProfileAsync()
    {
        if (IsCoach)
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
                ErrorMessage = "خطا در دریافت اطلاعات پروفایل مربی.";
            }
        }
        else
        {
            var userId = GetCurrentUserIdFromSession();
            if (userId == null) return;

            var profile = await _apiClient.GetAsync<UserProfileDto>($"api/users/{userId}");
            if (profile != null)
            {
                FirstName = profile.FirstName;
                LastName = profile.LastName;
                Email = profile.Email;
                Phone = profile.Phone;
                Goal = profile.Goal;
                Weight = profile.Weight;
                Height = profile.Height;
                BodyType = profile.BodyType;
                EstimatedCaloriesIntake = profile.EstimatedCaloriesIntake;
                IsCompetitive = profile.IsCompetitive;
                Username = profile.Username;        // اضافه شد
                CreatedAt = profile.CreatedAt.ToString("yyyy/MM/dd"); // اضافه شد
            }
            else
            {
                ErrorMessage = "خطا در دریافت اطلاعات پروفایل کاربر.";
            }
        }
    }

    private int? GetCurrentUserIdFromSession()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        Console.WriteLine($"[DEBUG] UserId from session: {userIdStr}");
        if (int.TryParse(userIdStr, out var userId))
            return userId;
        return null;
    }
}

// ========== DTOها ==========

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

public class UserProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Goal Goal { get; set; }
    public float? Weight { get; set; }
    public float? Height { get; set; }
    public BodyType? BodyType { get; set; }
    public int? EstimatedCaloriesIntake { get; set; }
    public bool IsCompetitive { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}