namespace GymFlow.Models.DTOs.Responses;

public class QuickStatsResponse
{
    public int TotalWorkouts { get; set; }
    public int CurrentStreak { get; set; }
    public int ConsistencyScore { get; set; }
    public float CurrentWeight { get; set; }
}