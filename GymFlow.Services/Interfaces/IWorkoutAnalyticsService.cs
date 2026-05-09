using GymFlow.Services.Models;

namespace GymFlow.Services.Interfaces;

public interface IWorkoutAnalyticsService
{
    Task<int> GetConsistencyScoreAsync(int userId, int weeks = 4);
    Task<int> GetCurrentStreakAsync(int userId);
    Task<int> GetLongestStreakAsync(int userId);
    Task<Dictionary<string, double>> GetCompletionRateByMuscleGroupAsync(int userId, int weeks = 4);
    Task<Dictionary<DayOfWeek, int>> GetBestWorkoutDaysAsync(int userId);
    Task<List<VolumePoint>> GetVolumeTrendAsync(int userId, int weeks = 8);
    Task<List<Achievement>> GetUserAchievementsAsync(int userId);
}