namespace GymFlow.Models.DTOs.Responses;

public class CoachListItemResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
}