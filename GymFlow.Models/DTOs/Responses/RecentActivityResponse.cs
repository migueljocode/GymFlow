namespace GymFlow.Models.DTOs.Responses;

public class RecentActivityResponse
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ClientName { get; set; }
}