using GymFlow.Models.DTOs.Responses;
using GymFlow.Services.Models;

namespace GymFlow.Services.Interfaces;

/// <summary>
/// Complete user dashboard data
/// </summary>
public class UserDashboardData
{
    public QuickStats Stats { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public List<Achievement> Achievements { get; set; } = new();
    public WorkoutPlanResponse? ActivePlan { get; set; }
    public PredictionResponse? WeightPrediction { get; set; }
    public WeeklySummaryResponse? WeeklySummary { get; set; }
    public List<WeightPointResponse> WeightHistory { get; set; } = new();
}

public interface IUserDashboardService
{
    Task<UserDashboardData> GetUserDashboardAsync(int userId);
    Task<QuickStats> GetQuickStatsAsync(int userId);
    Task<List<RecentActivity>> GetRecentActivitiesAsync(int userId, int count = 10);
}