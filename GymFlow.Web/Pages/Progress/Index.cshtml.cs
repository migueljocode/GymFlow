using Microsoft.AspNetCore.Mvc.RazorPages;
using GymFlow.Web.Services;

namespace GymFlow.Web.Pages.Progress;

public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    
    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public List<WeightLogDto> WeightHistory { get; set; } = new();
    public float CurrentWeight { get; set; }
    public float FirstWeight { get; set; }
    public float TotalChange { get; set; }
    
    public async Task OnGetAsync()
    {
        var userId = 1;
        WeightHistory = await _apiClient.GetAsync<List<WeightLogDto>>($"api/progress/user/{userId}") ?? new();
        
        if (WeightHistory.Any())
        {
            var ordered = WeightHistory.OrderByDescending(w => w.LogDate).ToList();
            CurrentWeight = ordered.First().Weight;
            FirstWeight = ordered.Last().Weight;
            TotalChange = CurrentWeight - FirstWeight;
        }
    }
}

public class WeightLogDto
{
    public DateOnly LogDate { get; set; }
    public float Weight { get; set; }
    public float? BodyFatPercentage { get; set; }
    public string? Notes { get; set; }
}