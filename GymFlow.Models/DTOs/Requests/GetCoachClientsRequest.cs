namespace GymFlow.Models.DTOs.Requests;

public class GetCoachClientsRequest
{
    public int CoachId { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? SearchTerm { get; set; }
}