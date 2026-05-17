using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;
using System.Text.Json.Serialization;
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

        var rawJson = await _apiClient.GetRawAsync($"api/coaches/{coachId}/clients");
        Console.WriteLine("=== RAW JSON FROM API ===");
        Console.WriteLine(rawJson);
        Console.WriteLine("=========================");

        if (!string.IsNullOrEmpty(rawJson))
        {
            using JsonDocument doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataArray.EnumerateArray())
                {
                    Console.WriteLine("--- ITEM ---");
                    foreach (var prop in item.EnumerateObject())
                    {
                        Console.WriteLine($"{prop.Name} : {prop.Value}");
                    }
                    Console.WriteLine("------------");
                    
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


    public async Task<IActionResult> OnPostDownloadProgressAsync(int userId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/progress/{userId}");
        return pdfBytes == null ? NotFound() : File(pdfBytes, "application/pdf", $"ProgressReport_User_{userId}.pdf");
    }

    public async Task<IActionResult> OnPostDownloadCertificateAsync(int userId)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/certificate/{userId}");
        return pdfBytes == null ? NotFound() : File(pdfBytes, "application/pdf", $"Certificate_User_{userId}.pdf");
    }

    public async Task<IActionResult> OnPostDownloadWeeklySummaryAsync(int userId, string weekStart)
    {
        var pdfBytes = await _apiClient.DownloadPdfAsync($"api/export/weekly-summary/{userId}?weekStart={weekStart}");
        return pdfBytes == null ? NotFound() : File(pdfBytes, "application/pdf", $"WeeklySummary_User_{userId}.pdf");
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