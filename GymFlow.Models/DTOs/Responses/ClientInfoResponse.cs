namespace GymFlow.Models.DTOs.Responses;

public class ClientInfoResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Goal { get; set; } = string.Empty;
    public float CurrentWeight { get; set; }
    public int CompletedSessions { get; set; }
}