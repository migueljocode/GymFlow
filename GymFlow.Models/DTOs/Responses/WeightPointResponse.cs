namespace GymFlow.Models.DTOs.Responses;

public class WeightPointResponse
{
    public DateOnly Date { get; set; }
    public float Weight { get; set; }
    public float? BodyFatPercentage { get; set; }
}