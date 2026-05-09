namespace GymFlow.Services.Models;

/// <summary>
/// Represents a user achievement/badge earned during workouts
/// </summary>
public class Achievement
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; }
    public string Icon { get; set; } = string.Empty;
    public bool IsNew { get; set; }
}

/// <summary>
/// Represents a point in volume trend analysis
/// </summary>
public class VolumePoint
{
    public DateOnly Date { get; set; }
    public int TotalVolume { get; set; }
    public string MuscleGroup { get; set; } = string.Empty;
}

/// <summary>
/// Quick stats for dashboard overview
/// </summary>
public class QuickStats
{
    public int TotalWorkouts { get; set; }
    public int WorkoutsThisWeek { get; set; }
    public int CurrentStreak { get; set; }
    public int ConsistencyScore { get; set; }
    public float CurrentWeight { get; set; }
    public float TotalWeightChange { get; set; }
    public int TotalWorkoutMinutes { get; set; }
    public int AchievementsCount { get; set; }
}

/// <summary>
/// Recent activity item for dashboard timeline
/// </summary>
public class RecentActivity
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
}