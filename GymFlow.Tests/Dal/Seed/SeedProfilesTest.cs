using Xunit;
using GymFlow.Dal.Seed.Data;

namespace GymFlow.Tests.Dal.Seed.Data;

public class SeedProfilesTest
{
    // ========== Development Profile Tests ==========

    [Fact]
    public void DevelopmentProfile_ShouldHaveCorrectValues()
    {
        // Act
        var profile = SeedProfiles.Development;

        // Assert
        Assert.True(profile.RefreshOnStartup);
        Assert.True(profile.ClearExistingData);
        Assert.False(profile.SeedOnlyIfEmpty);
        Assert.Equal(15, profile.UserCount);
        Assert.True(profile.IncludeDemoUser);
        Assert.Equal(2, profile.MinWorkoutPlansPerUser);
        Assert.Equal(4, profile.MaxWorkoutPlansPerUser);
        Assert.Equal(3, profile.MinWorkoutDaysPerPlan);
        Assert.Equal(6, profile.MaxWorkoutDaysPerPlan);
        Assert.Equal(4, profile.MinExercisesPerDay);
        Assert.Equal(8, profile.MaxExercisesPerDay);
        Assert.Equal(15, profile.MinProgressLogsPerUser);
        Assert.Equal(45, profile.MaxProgressLogsPerUser);
        Assert.Equal(0.65, profile.WorkoutSessionCompletionRate);
        Assert.Equal(0.7, profile.BodyFatPercentageInclusionRate);
        Assert.Equal(0.8, profile.FeelingNoteProbability);
        Assert.Equal(0.3, profile.CompoundWorkoutProbability);
        Assert.Equal(1337, profile.RandomSeed);
    }

    // ========== QuickDemo Profile Tests ==========

    [Fact]
    public void QuickDemoProfile_ShouldHaveCorrectValues()
    {
        // Act
        var profile = SeedProfiles.QuickDemo;

        // Assert
        Assert.True(profile.RefreshOnStartup);
        Assert.True(profile.ClearExistingData);
        Assert.False(profile.SeedOnlyIfEmpty);
        Assert.Equal(3, profile.UserCount);
        Assert.True(profile.IncludeDemoUser);
        Assert.Equal(1, profile.MinWorkoutPlansPerUser);
        Assert.Equal(1, profile.MaxWorkoutPlansPerUser);
        Assert.Equal(3, profile.MinWorkoutDaysPerPlan);
        Assert.Equal(3, profile.MaxWorkoutDaysPerPlan);
        Assert.Equal(4, profile.MinExercisesPerDay);
        Assert.Equal(6, profile.MaxExercisesPerDay);
        Assert.Equal(5, profile.MinProgressLogsPerUser);
        Assert.Equal(10, profile.MaxProgressLogsPerUser);
        Assert.Equal(0.8, profile.WorkoutSessionCompletionRate);
        Assert.Equal(0.7, profile.BodyFatPercentageInclusionRate);
        Assert.Equal(0.8, profile.FeelingNoteProbability);
        Assert.Equal(0.2, profile.CompoundWorkoutProbability);
        Assert.Equal(42, profile.RandomSeed);
    }

    // ========== Lightweight Profile Tests ==========

    [Fact]
    public void LightweightProfile_ShouldHaveCorrectValues()
    {
        // Act
        var profile = SeedProfiles.Lightweight;

        // Assert
        Assert.True(profile.RefreshOnStartup);
        Assert.True(profile.ClearExistingData);
        Assert.False(profile.SeedOnlyIfEmpty);
        Assert.Equal(5, profile.UserCount);
        Assert.True(profile.IncludeDemoUser);
        Assert.Equal(1, profile.MinWorkoutPlansPerUser);
        Assert.Equal(2, profile.MaxWorkoutPlansPerUser);
        Assert.Equal(2, profile.MinWorkoutDaysPerPlan);
        Assert.Equal(3, profile.MaxWorkoutDaysPerPlan);
        Assert.Equal(3, profile.MinExercisesPerDay);
        Assert.Equal(5, profile.MaxExercisesPerDay);
        Assert.Equal(5, profile.MinProgressLogsPerUser);
        Assert.Equal(15, profile.MaxProgressLogsPerUser);
        Assert.Equal(0.7, profile.WorkoutSessionCompletionRate);
        Assert.Equal(0.5, profile.BodyFatPercentageInclusionRate);
        Assert.Equal(0.6, profile.FeelingNoteProbability);
        Assert.Equal(0.1, profile.CompoundWorkoutProbability);
        Assert.Equal(123, profile.RandomSeed);
    }

    // ========== StressTest Profile Tests ==========

    [Fact]
    public void StressTestProfile_ShouldHaveCorrectValues()
    {
        // Act
        var profile = SeedProfiles.StressTest;

        // Assert
        Assert.True(profile.RefreshOnStartup);
        Assert.True(profile.ClearExistingData);
        Assert.False(profile.SeedOnlyIfEmpty);
        Assert.Equal(50, profile.UserCount);
        Assert.True(profile.IncludeDemoUser);
        Assert.Equal(3, profile.MinWorkoutPlansPerUser);
        Assert.Equal(5, profile.MaxWorkoutPlansPerUser);
        Assert.Equal(4, profile.MinWorkoutDaysPerPlan);
        Assert.Equal(6, profile.MaxWorkoutDaysPerPlan);
        Assert.Equal(5, profile.MinExercisesPerDay);
        Assert.Equal(10, profile.MaxExercisesPerDay);
        Assert.Equal(30, profile.MinProgressLogsPerUser);
        Assert.Equal(60, profile.MaxProgressLogsPerUser);
        Assert.Equal(0.6, profile.WorkoutSessionCompletionRate);
        Assert.Equal(0.8, profile.BodyFatPercentageInclusionRate);
        Assert.Equal(0.9, profile.FeelingNoteProbability);
        Assert.Equal(0.4, profile.CompoundWorkoutProbability);
        Assert.Null(profile.RandomSeed);
    }

    // ========== Production Profile Tests ==========

    [Fact]
    public void ProductionProfile_ShouldHaveCorrectValues()
    {
        // Act
        var profile = SeedProfiles.Production;

        // Assert
        Assert.False(profile.RefreshOnStartup);
        Assert.False(profile.ClearExistingData);
        Assert.True(profile.SeedOnlyIfEmpty);
        Assert.Equal(0, profile.UserCount);
        Assert.False(profile.IncludeDemoUser);
        Assert.Equal(0, profile.MinWorkoutPlansPerUser);
        Assert.Equal(0, profile.MaxWorkoutPlansPerUser);
        Assert.Equal(0, profile.MinWorkoutDaysPerPlan);
        Assert.Equal(0, profile.MaxWorkoutDaysPerPlan);
        Assert.Equal(0, profile.MinExercisesPerDay);
        Assert.Equal(0, profile.MaxExercisesPerDay);
        Assert.Equal(0, profile.MinProgressLogsPerUser);
        Assert.Equal(0, profile.MaxProgressLogsPerUser);
        Assert.Equal(0, profile.WorkoutSessionCompletionRate);
        Assert.Equal(0, profile.BodyFatPercentageInclusionRate);
        Assert.Equal(0, profile.FeelingNoteProbability);
        Assert.Equal(0, profile.CompoundWorkoutProbability);
        Assert.Null(profile.RandomSeed);
    }

    // ========== Empty Profile Tests ==========

    [Fact]
    public void EmptyProfile_ShouldHaveCorrectValues()
    {
        // Act
        var profile = SeedProfiles.Empty;

        // Assert
        Assert.False(profile.RefreshOnStartup);
        Assert.True(profile.ClearExistingData);
        Assert.False(profile.SeedOnlyIfEmpty);
        Assert.Equal(0, profile.UserCount);
        Assert.False(profile.IncludeDemoUser);
        Assert.Equal(0, profile.MinWorkoutPlansPerUser);
        Assert.Equal(0, profile.MaxWorkoutPlansPerUser);
        Assert.Equal(0, profile.MinWorkoutDaysPerPlan);
        Assert.Equal(0, profile.MaxWorkoutDaysPerPlan);
        Assert.Equal(0, profile.MinExercisesPerDay);
        Assert.Equal(0, profile.MaxExercisesPerDay);
        Assert.Equal(0, profile.MinProgressLogsPerUser);
        Assert.Equal(0, profile.MaxProgressLogsPerUser);
        Assert.Equal(0, profile.WorkoutSessionCompletionRate);
        Assert.Equal(0, profile.BodyFatPercentageInclusionRate);
        Assert.Equal(0, profile.FeelingNoteProbability);
        Assert.Equal(0, profile.CompoundWorkoutProbability);
        Assert.Null(profile.RandomSeed);
    }

    // ========== GetByName Tests ==========

    [Theory]
    [InlineData("development", "Development")]
    [InlineData("dev", "Development")]
    [InlineData("Development", "Development")]
    [InlineData("quickdemo", "QuickDemo")]
    [InlineData("demo", "QuickDemo")]
    [InlineData("QuickDemo", "QuickDemo")]
    [InlineData("lightweight", "Lightweight")]
    [InlineData("light", "Lightweight")]
    [InlineData("Lightweight", "Lightweight")]
    [InlineData("stresstest", "StressTest")]
    [InlineData("stress", "StressTest")]
    [InlineData("performance", "StressTest")]
    [InlineData("production", "Production")]
    [InlineData("prod", "Production")]
    [InlineData("empty", "Empty")]
    [InlineData("clean", "Empty")]
    [InlineData("none", "Empty")]
    public void GetByName_WithValidName_ShouldReturnCorrectProfile(string input, string expectedProfile)
    {
        // Act
        var profile = SeedProfiles.GetByName(input);

        // Assert
        Assert.NotNull(profile);
        
        switch (expectedProfile)
        {
            case "Development":
                Assert.Equal(15, profile.UserCount);
                break;
            case "QuickDemo":
                Assert.Equal(3, profile.UserCount);
                break;
            case "Lightweight":
                Assert.Equal(5, profile.UserCount);
                break;
            case "StressTest":
                Assert.Equal(50, profile.UserCount);
                break;
            case "Production":
                Assert.Equal(0, profile.UserCount);
                Assert.False(profile.IncludeDemoUser);
                break;
            case "Empty":
                Assert.Equal(0, profile.UserCount);
                Assert.False(profile.IncludeDemoUser);
                break;
        }
    }

    [Fact]
    public void GetByName_WithInvalidName_ShouldReturnDevelopmentProfile()
    {
        // Act
        var profile = SeedProfiles.GetByName("invalidname");

        // Assert
        Assert.NotNull(profile);
        Assert.Equal(15, profile.UserCount);
        Assert.True(profile.IncludeDemoUser);
    }

    [Fact]
    public void GetByName_WithNull_ShouldReturnDevelopmentProfile()
    {
        // Act - استفاده از null! برای suppressing warning
        var profile = SeedProfiles.GetByName(null!);

        // Assert
        Assert.NotNull(profile);
        Assert.Equal(15, profile.UserCount);
    }

    [Fact]
    public void GetByName_WithEmptyString_ShouldReturnDevelopmentProfile()
    {
        // Act
        var profile = SeedProfiles.GetByName("");

        // Assert
        Assert.NotNull(profile);
        Assert.Equal(15, profile.UserCount);
    }

    // ========== AvailableProfiles Tests ==========

    [Fact]
    public void AvailableProfiles_ShouldContainAllProfiles()
    {
        // Act
        var profiles = SeedProfiles.AvailableProfiles;

        // Assert
        Assert.NotNull(profiles);
        Assert.Equal(6, profiles.Count);
        Assert.Contains("Development", profiles);
        Assert.Contains("QuickDemo", profiles);
        Assert.Contains("Lightweight", profiles);
        Assert.Contains("StressTest", profiles);
        Assert.Contains("Production", profiles);
        Assert.Contains("Empty", profiles);
    }

    [Fact]
    public void AvailableProfiles_ShouldBeReadOnlyList()
    {
        // Act
        var profiles = SeedProfiles.AvailableProfiles;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<string>>(profiles);
    }

    // ========== Profile Uniqueness Tests ==========

    [Fact]
    public void AllProfiles_ShouldBeDifferentInstances()
    {
        // Act
        var development = SeedProfiles.Development;
        var quickDemo = SeedProfiles.QuickDemo;
        var lightweight = SeedProfiles.Lightweight;
        var stressTest = SeedProfiles.StressTest;
        var production = SeedProfiles.Production;
        var empty = SeedProfiles.Empty;

        // Assert
        Assert.NotSame(development, quickDemo);
        Assert.NotSame(development, lightweight);
        Assert.NotSame(development, stressTest);
        Assert.NotSame(development, production);
        Assert.NotSame(development, empty);
        Assert.NotSame(quickDemo, lightweight);
        Assert.NotSame(quickDemo, stressTest);
        Assert.NotSame(quickDemo, production);
        Assert.NotSame(quickDemo, empty);
        Assert.NotSame(lightweight, stressTest);
        Assert.NotSame(lightweight, production);
        Assert.NotSame(lightweight, empty);
        Assert.NotSame(stressTest, production);
        Assert.NotSame(stressTest, empty);
        Assert.NotSame(production, empty);
    }

    [Fact]
    public void DevelopmentAndQuickDemo_ShouldHaveDifferentUserCounts()
    {
        // Assert
        Assert.NotEqual(SeedProfiles.Development.UserCount, SeedProfiles.QuickDemo.UserCount);
        Assert.NotEqual(SeedProfiles.Development.MinWorkoutPlansPerUser, SeedProfiles.QuickDemo.MinWorkoutPlansPerUser);
    }

    [Fact]
    public void ProductionAndEmpty_ShouldHaveDifferentClearExistingData()
    {
        // Assert
        Assert.NotEqual(SeedProfiles.Production.ClearExistingData, SeedProfiles.Empty.ClearExistingData);
        Assert.NotEqual(SeedProfiles.Production.SeedOnlyIfEmpty, SeedProfiles.Empty.SeedOnlyIfEmpty);
    }
}