namespace GymFlow.Web.Pages.Progress;

public class IndexModel : BasePageModel
{
    private readonly ApiClient _apiClient;
    
    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    
    public List<ProgressLogResponse> WeightHistory { get; set; } = new();
    public float CurrentWeight { get; set; }
    public float FirstWeight { get; set; }
    public float TotalChange { get; set; }
    public int? ClientId { get; set; }
    
    public async Task<IActionResult> OnGetAsync(int? userId = null)
    {
        int targetUserId;
        
        if (IsCoach && userId.HasValue && userId.Value > 0)
        {
            targetUserId = userId.Value;
            ClientId = userId.Value;
        }
        else
        {
            if (!int.TryParse(HttpContext.Session.GetString("UserId"), out targetUserId))
                return RedirectToPage("/Login");
            if (!IsCoach)
            {
                var redirect = RedirectIfNotMember();
                if (redirect != null) return redirect;
            }
        }
        
        WeightHistory = await _apiClient.GetAsync<List<ProgressLogResponse>>($"api/progress/user/{targetUserId}") ?? new();
        
        if (WeightHistory.Any())
        {
            var ordered = WeightHistory.OrderByDescending(w => w.LogDate).ToList();
            CurrentWeight = ordered.First().Weight;
            FirstWeight = ordered.Last().Weight;
            TotalChange = CurrentWeight - FirstWeight;
        }
        return Page();
    }
}