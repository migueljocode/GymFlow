using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;
using System.Text.Json;

namespace GymFlow.Web.Pages.Coach;

public class ClientsModel : BasePageModel
{
    private readonly ApiClient _apiClient;

    public ClientsModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<ClientInfoDto> Clients { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RedirectIfNotCoach();
        if (redirect != null) return redirect;

        if (!int.TryParse(HttpContext.Session.GetString("UserId"), out var coachId))
            return RedirectToPage("/Login");

        // دریافت داده از API به صورت Raw
        var rawJson = await GetRawApiResponseAsync($"api/coaches/{coachId}/clients");
        
        if (!string.IsNullOrEmpty(rawJson))
        {
            using JsonDocument doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;
            
            // بررسی ساختار پاسخ: { success: true, data: [...] }
            if (root.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataArray.EnumerateArray())
                {
                    var dto = new ClientInfoDto
                    {
                        Id = item.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0,
                        FullName = item.TryGetProperty("fullName", out var nameProp) ? nameProp.GetString() ?? "Unknown" : "Unknown",
                        Goal = item.TryGetProperty("goal", out var goalProp) ? goalProp.GetString() ?? "نامشخص" : "نامشخص",
                        CurrentWeight = item.TryGetProperty("currentWeight", out var weightProp) ? (float)weightProp.GetDouble() : 0,
                        CompletedSessions = item.TryGetProperty("completedSessions", out var sessionsProp) ? sessionsProp.GetInt32() : 0
                    };
                    Clients.Add(dto);
                }
            }
        }

        return Page();
    }

    private async Task<string?> GetRawApiResponseAsync(string url)
    {
        try
        {
            using var client = new HttpClient();
            var baseUrl = "http://localhost:5291/"; // از تنظیمات بگیرید
            var fullUrl = $"{baseUrl}{url}";
            
            // اضافه کردن توکن احراز هویت
            var token = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }
            
            var response = await client.GetAsync(fullUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting raw API response: {ex.Message}");
            return null;
        }
    }

    public async Task<IActionResult> OnPostDownloadProgressAsync(int userId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/progress/{userId}");
        if (pdfBytes == null) return NotFound();
        return File(pdfBytes, "application/pdf", $"ProgressReport_User_{userId}.pdf");
    }

    public async Task<IActionResult> OnPostDownloadCertificateAsync(int userId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/certificate/{userId}");
        if (pdfBytes == null) return NotFound();
        return File(pdfBytes, "application/pdf", $"Certificate_User_{userId}.pdf");
    }

    public async Task<IActionResult> OnPostDownloadWeeklySummaryAsync(int userId, string weekStart)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/weekly-summary/{userId}?weekStart={weekStart}");
        if (pdfBytes == null) return NotFound();
        return File(pdfBytes, "application/pdf", $"WeeklySummary_User_{userId}.pdf");
    }
}

public class ClientInfoDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public float CurrentWeight { get; set; }
    public int CompletedSessions { get; set; }
}