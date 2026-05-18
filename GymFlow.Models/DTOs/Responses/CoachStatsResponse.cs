namespace GymFlow.Models.DTOs.Responses;

public class CoachStatsResponse
{
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public int TotalWorkoutsThisWeek { get; set; }
    public int TotalWorkoutsThisMonth { get; set; }
    public float AverageClientWeight { get; set; }
    public int PlansCreated { get; set; }
}