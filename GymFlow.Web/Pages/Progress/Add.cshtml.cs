using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;
using GymFlow.Models.DTOs.Requests;

namespace GymFlow.Web.Pages.Progress;

public class AddModel : PageModel
{
    private readonly ApiClient _apiClient;
    
    public AddModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    [BindProperty]
    public DateOnly LogDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    [BindProperty]
    public float Weight { get; set; }
    
    [BindProperty]
    public float? BodyFatPercentage { get; set; }
    
    [BindProperty]
    public string? Notes { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            TempData["ErrorMessage"] = "لطفاً مجدداً وارد شوید.";
            return RedirectToPage("/Login");
        }

        if (Weight < 20 || Weight > 300)
        {
            TempData["ErrorMessage"] = "وزن باید بین ۲۰ تا ۳۰۰ کیلوگرم باشد";
            return RedirectToPage();
        }
        
        if (BodyFatPercentage.HasValue && (BodyFatPercentage < 3 || BodyFatPercentage > 50))
        {
            TempData["ErrorMessage"] = "درصد چربی باید بین ۳ تا ۵۰ باشد";
            return RedirectToPage();
        }
        
        if (LogDate > DateOnly.FromDateTime(DateTime.Now))
        {
            TempData["ErrorMessage"] = "تاریخ نمی‌تواند در آینده باشد";
            return RedirectToPage();
        }
        
        var request = new CreateProgressLogRequest
        {
            LogDate = LogDate,
            Weight = Weight,
            BodyFatPercentage = BodyFatPercentage,
            Notes = Notes
        };
        
        var (result, errorMessage) = await _apiClient.PostWithErrorAsync<ProgressLogResponse>($"api/progress/user/{userId}", request);
        
        if (result != null)
        {
            TempData["Message"] = $"✅ وزن {Weight} کیلوگرم با موفقیت ثبت شد! 📊";
            return RedirectToPage("/Progress/Index");
        }
        
        TempData["ErrorMessage"] = errorMessage ?? "❌ خطا در ثبت وزن. لطفاً دوباره تلاش کنید.";
        return RedirectToPage();
    }
}

public class ProgressLogResponse
{
    public int Id { get; set; }
    public DateOnly LogDate { get; set; }
    public float Weight { get; set; }
}