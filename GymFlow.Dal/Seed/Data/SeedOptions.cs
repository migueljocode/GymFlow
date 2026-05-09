namespace GymFlow.Dal.Seed.Data;

/// <summary>
/// Configuration options for database seeding behavior.
/// </summary>
public class SeedOptions
{
    /// <summary>
    /// Whether to refresh data on every development startup (default: true).
    /// </summary>
    public bool RefreshOnStartup { get; set; } = true;
    
    /// <summary>
    /// Whether to clear existing data before seeding (default: true in Development).
    /// </summary>
    public bool ClearExistingData { get; set; } = true;
    
    /// <summary>
    /// Whether to only seed if database is empty (default: false in Development).
    /// </summary>
    public bool SeedOnlyIfEmpty { get; set; } = false;
    
    // ========== USER GENERATION ==========
    public int UserCount { get; set; } = 15;
    public bool IncludeDemoUser { get; set; } = true;
    public string DemoUserEmail { get; set; } = "demo@gymflow.com";
    public string DemoUserFirstName { get; set; } = "Demo";
    public string DemoUserLastName { get; set; } = "User";
    
    // ========== WORKOUT PLAN GENERATION ==========
    public int MinWorkoutPlansPerUser { get; set; } = 2;
    public int MaxWorkoutPlansPerUser { get; set; } = 4;
    public int MinWorkoutDaysPerPlan { get; set; } = 3;
    public int MaxWorkoutDaysPerPlan { get; set; } = 6;
    public int MinExercisesPerDay { get; set; } = 4;
    public int MaxExercisesPerDay { get; set; } = 8;
    public double CompoundWorkoutProbability { get; set; } = 0.3;
    
    // ========== PROGRESS LOG GENERATION ==========
    public int MinProgressLogsPerUser { get; set; } = 15;
    public int MaxProgressLogsPerUser { get; set; } = 45;
    public double BodyFatPercentageInclusionRate { get; set; } = 0.7;
    
    // ========== WORKOUT SESSION GENERATION ==========
    public double WorkoutSessionCompletionRate { get; set; } = 0.65;
    public double FeelingNoteProbability { get; set; } = 0.8;
    
    // ========== REALISM & VARIETY ==========
    public int? RandomSeed { get; set; } = 1337;
    public (float Min, float Max) ActiveWeightChangeRange { get; set; } = (-0.7f, -0.3f);
    public (float Min, float Max) BreakWeightChangeRange { get; set; } = (0.2f, 0.5f);
    
    // ========== Create from profile ==========
    
    /// <summary>
    /// Creates SeedOptions from a predefined profile.
    /// </summary>
    public static SeedOptions FromProfile(SeedOptions profile) => new()
    {
        RefreshOnStartup = profile.RefreshOnStartup,
        ClearExistingData = profile.ClearExistingData,
        SeedOnlyIfEmpty = profile.SeedOnlyIfEmpty,
        UserCount = profile.UserCount,
        IncludeDemoUser = profile.IncludeDemoUser,
        DemoUserEmail = profile.DemoUserEmail,
        DemoUserFirstName = profile.DemoUserFirstName,
        DemoUserLastName = profile.DemoUserLastName,
        MinWorkoutPlansPerUser = profile.MinWorkoutPlansPerUser,
        MaxWorkoutPlansPerUser = profile.MaxWorkoutPlansPerUser,
        MinWorkoutDaysPerPlan = profile.MinWorkoutDaysPerPlan,
        MaxWorkoutDaysPerPlan = profile.MaxWorkoutDaysPerPlan,
        MinExercisesPerDay = profile.MinExercisesPerDay,
        MaxExercisesPerDay = profile.MaxExercisesPerDay,
        CompoundWorkoutProbability = profile.CompoundWorkoutProbability,
        MinProgressLogsPerUser = profile.MinProgressLogsPerUser,
        MaxProgressLogsPerUser = profile.MaxProgressLogsPerUser,
        BodyFatPercentageInclusionRate = profile.BodyFatPercentageInclusionRate,
        WorkoutSessionCompletionRate = profile.WorkoutSessionCompletionRate,
        FeelingNoteProbability = profile.FeelingNoteProbability,
        RandomSeed = profile.RandomSeed,
        ActiveWeightChangeRange = profile.ActiveWeightChangeRange,
        BreakWeightChangeRange = profile.BreakWeightChangeRange
    };
}