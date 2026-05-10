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
    
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (Weight < 20 || Weight > 300)
        {
            ErrorMessage = "وزن باید بین ۲۰ تا ۳۰۰ کیلوگرم باشد";
            return Page();
        }
        
        if (BodyFatPercentage.HasValue && (BodyFatPercentage < 3 || BodyFatPercentage > 50))
        {
            ErrorMessage = "درصد چربی باید بین ۳ تا ۵۰ باشد";
            return Page();
        }
        
        if (LogDate > DateOnly.FromDateTime(DateTime.Now))
        {
            ErrorMessage = "تاریخ نمی‌تواند در آینده باشد";
            return Page();
        }
        
        var users = await _apiClient.GetAsync<List<UserDto>>("api/users");
        var userId = users?.FirstOrDefault()?.Id ?? 1;
        
        var request = new CreateProgressLogRequest
        {
            LogDate = LogDate,
            Weight = Weight,
            BodyFatPercentage = BodyFatPercentage,
            Notes = Notes
        };
        
        var result = await _apiClient.PostAsync<ProgressLogResponse>($"api/progress/user/{userId}", request);
        
        if (result != null)
        {
            Message = $"وزن {Weight} کیلوگرم با موفقیت ثبت شد! 📊";
            Weight = 0;
            BodyFatPercentage = null;
            Notes = null;
            return Page();
        }
        
        ErrorMessage = "خطا در ثبت وزن. لطفاً دوباره تلاش کنید.";
        return Page();
    }
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class ProgressLogResponse
{
    public int Id { get; set; }
    public DateOnly LogDate { get; set; }
    public float Weight { get; set; }
}