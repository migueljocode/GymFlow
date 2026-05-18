namespace GymFlow.Models.DTOs.Responses;

public class ApiErrorResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; }
}