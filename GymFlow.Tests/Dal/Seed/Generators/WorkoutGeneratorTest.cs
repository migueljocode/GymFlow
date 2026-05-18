namespace GymFlow.Tests.Dal.Seed.Generators;

public class WorkoutGeneratorTest
{
    private readonly SeedOptions _options;

    public WorkoutGeneratorTest()
    {
        _options = SeedProfiles.Lightweight;
        _options.RandomSeed = 42;
    }

    // ========== Helper Methods ==========

    private User CreateTestUser(int id = 1)
    {
        var person = new Person
        {
            Id = id,
            FirstName = "Test",
            LastName = "User",
            Username = $"testuser{id}",
            Password = "password123",
            Email = $"testuser{id}@test.com",
            Gender = Gender.Male,
            Age = 30,
            Weight = 80f,
            Height = 180f,
            BodyType = BodyType.Fit,
            CreatedAt = DateTime.UtcNow
        };

        return new User
        {
            Id = id,
            PersonId = person.Id,
            Person = person,
            Goal = Goal.Fitness,
            CreatedAt = DateTime.UtcNow
        };
    }

    private WorkoutGenerator CreateGenerator(int startPlanId = 1, int startWorkoutDayId = 1)
    {
        return new WorkoutGenerator(_options, startPlanId, startWorkoutDayId);
    }

    // ========== Tests for GenerateExercises ==========

    [Fact]
    public void GenerateExercises_ShouldReturnAllExercisesFromLibrary()
    {
        // Arrange
        var generator = CreateGenerator();
        var expectedCount = ExerciseLibrary.GetAllExercises().Count;

        // Act
        var exercises = generator.GenerateExercises();

        // Assert
        Assert.NotNull(exercises);
        Assert.Equal(expectedCount, exercises.Count);
    }

    [Fact]
    public void GenerateExercises_ShouldHaveUniqueIds()
    {
        // Arrange
        var generator = CreateGenerator();

        // Act
        var exercises = generator.GenerateExercises();
        var ids = exercises.Select(e => e.Id).ToList();
        var distinctIds = ids.Distinct().ToList();

        // Assert
        Assert.Equal(ids.Count, distinctIds.Count);
        Assert.Equal(Enumerable.Range(1, exercises.Count).ToList(), ids.OrderBy(x => x).ToList());
    }

    [Fact]
    public void GenerateExercises_ShouldHaveValidProperties()
    {
        // Arrange
        var generator = CreateGenerator();
        var libraryExercises = ExerciseLibrary.GetAllExercises();

        // Act
        var exercises = generator.GenerateExercises();

        // Assert
        for (int i = 0; i < exercises.Count; i++)
        {
            var exercise = exercises[i];
            var template = libraryExercises[i];
            
            Assert.Equal(template.Name, exercise.Name);
            Assert.Equal(template.MuscleGroup, exercise.PrimaryMuscleGroup);
            Assert.Contains(template.Description, exercise.Description);
            Assert.Contains(template.Equipment, exercise.Description);
            Assert.True(exercise.CreatedAt <= DateTime.UtcNow);
            Assert.True(exercise.CreatedAt >= DateTime.UtcNow.AddMonths(-12).AddDays(-1));
        }
    }

    [Fact]
    public void GenerateExercises_ShouldHaveValidMuscleGroups()
    {
        // Arrange
        var generator = CreateGenerator();
        var validMuscleGroups = new[] 
        { 
            MuscleGroup.Chest, MuscleGroup.Back, MuscleGroup.Legs, 
            MuscleGroup.Shoulders, MuscleGroup.Arms, MuscleGroup.Core 
        };

        // Act
        var exercises = generator.GenerateExercises();

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.Contains(exercise.PrimaryMuscleGroup, validMuscleGroups);
        }
    }

    // ========== Tests for GenerateWorkoutPlans ==========

    [Fact]
    public void GenerateWorkoutPlans_ShouldGenerateCorrectNumberOfPlans()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 3;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);

        // Assert
        Assert.NotNull(plans);
        Assert.Equal(planCount, plans.Count);
    }

    [Fact]
    public void GenerateWorkoutPlans_ShouldHaveCorrectPhases()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 4;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);

        // Assert
        for (int i = 0; i < planCount; i++)
        {
            Assert.Equal(i + 1, plans[i].Phase);
        }
    }

    [Fact]
    public void GenerateWorkoutPlans_LastPlanShouldBeActive()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 3;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);

        // Assert
        for (int i = 0; i < planCount - 1; i++)
        {
            Assert.False(plans[i].IsActive);
        }
        Assert.True(plans.Last().IsActive);
    }

    [Fact]
    public void GenerateWorkoutPlans_LastPlanShouldHaveNullEndDate()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 3;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);

        // Assert
        for (int i = 0; i < planCount - 1; i++)
        {
            Assert.NotNull(plans[i].EndDate);
        }
        Assert.Null(plans.Last().EndDate);
    }

    [Fact]
    public void GenerateWorkoutPlans_ShouldHaveValidSessionPerWeekRange()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 5;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);

        // Assert
        foreach (var plan in plans)
        {
            Assert.InRange(plan.SessionsPerWeek, 3, 5);
        }
    }

    [Fact]
    public void GenerateWorkoutPlans_ShouldHaveValidStartDates()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 3;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);

        // Assert
        for (int i = 1; i < plans.Count; i++)
        {
            Assert.True(plans[i].StartDate > plans[i - 1].StartDate);
        }
    }

    [Fact]
    public void GenerateWorkoutPlans_ShouldIncrementPlanId()
    {
        // Arrange
        var generator = CreateGenerator(10, 1);
        var user = CreateTestUser(1);
        var planCount = 3;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);

        // Assert
        Assert.Equal(10, plans[0].Id);
        Assert.Equal(11, plans[1].Id);
        Assert.Equal(12, plans[2].Id);
        Assert.Equal(13, generator.CurrentPlanId);
    }

    [Fact]
    public void GenerateWorkoutPlans_MayHaveNotes()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 10;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);
        var hasNotes = plans.Any(p => !string.IsNullOrEmpty(p.Notes));

        // Assert
        Assert.True(hasNotes, "Expected at least some plans to have notes (40% probability)");
    }

    // ========== Tests for GenerateWorkoutDays ==========

    [Fact]
    public void GenerateWorkoutDays_ShouldGenerateCorrectNumberOfDays()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 1);
        var plan = plans.First();
        var dayCount = plan.SessionsPerWeek;

        // Act
        var days = generator.GenerateWorkoutDays(plan, dayCount);

        // Assert
        Assert.NotNull(days);
        Assert.Equal(dayCount, days.Count);
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldHaveValidDayOfWeek()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 1);
        var plan = plans.First();
        var validDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
                                 DayOfWeek.Thursday, DayOfWeek.Friday };

        // Act
        var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);

        // Assert
        foreach (var day in days)
        {
            Assert.Contains(day.DayOfWeek, validDays);
        }
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldHaveNoDuplicateWeekdays()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 1);
        var plan = plans.First();

        // Act
        var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);
        var weekdays = days.Select(d => d.DayOfWeek).ToList();
        var distinctWeekdays = weekdays.Distinct().ToList();

        // Assert
        Assert.Equal(weekdays.Count, distinctWeekdays.Count);
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldBeOrderedByDayOfWeek()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 1);
        var plan = plans.First();

        // Act
        var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);

        // Assert
        for (int i = 1; i < days.Count; i++)
        {
            Assert.True(days[i].DayOfWeek > days[i - 1].DayOfWeek);
        }
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldHaveValidDurationRange()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 1);
        var plan = plans.First();

        // Act
        var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);

        // Assert
        foreach (var day in days)
        {
            Assert.InRange(day.DurationMinutes, 45, 90);
        }
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldHaveValidIntensity()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 1);
        var plan = plans.First();
        var validIntensities = new[] { Intensity.Low, Intensity.Medium, Intensity.High };

        // Act
        var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);

        // Assert
        foreach (var day in days)
        {
            Assert.Contains(day.Intensity, validIntensities);
        }
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldHaveValidTargetMuscles()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 1);
        var plan = plans.First();

        // Act
        var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);

        // Assert
        foreach (var day in days)
        {
            Assert.NotEqual(MuscleGroup.None, day.TargetMuscles);
        }
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldUsePlanCreatedAt()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 1);
        var plan = plans.First();

        // Act
        var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);

        // Assert
        foreach (var day in days)
        {
            Assert.Equal(plan.CreatedAt, day.CreatedAt);
        }
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldIncrementWorkoutDayId()
    {
        // Arrange
        var generator = CreateGenerator(1, 50);
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 1);
        var plan = plans.First();

        // Act
        var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);

        // Assert
        Assert.Equal(50, days[0].Id);
        if (days.Count > 1)
        {
            Assert.Equal(51, days[1].Id);
        }
        Assert.Equal(50 + days.Count, generator.CurrentWorkoutDayId);
    }

    [Fact]
    public void GenerateWorkoutDays_MayHaveNotes()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var plans = generator.GenerateWorkoutPlans(user, 5);
        var allDays = new List<WorkoutDay>();

        // Act
        foreach (var plan in plans)
        {
            var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);
            allDays.AddRange(days);
        }
        
        var hasNotes = allDays.Any(d => !string.IsNullOrEmpty(d.Notes));

        // Assert - 20% احتمال
        Assert.True(hasNotes, "Expected at least some workout days to have notes");
    }

    // ========== Tests for Private Methods (via Reflection) ==========

    [Fact]
    public void GetWorkoutSplits_ShouldReturnCorrectSplitsFor3Sessions()
    {
        // Arrange
        var generator = CreateGenerator();
        var methodInfo = typeof(WorkoutGenerator).GetMethod("GetWorkoutSplits", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.NotNull(methodInfo);

        // Act
        var result = methodInfo.Invoke(generator, new object[] { 3 }) as List<MuscleGroup>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(MuscleGroup.Chest | MuscleGroup.Arms, result);
        Assert.Contains(MuscleGroup.Back | MuscleGroup.Shoulders, result);
        Assert.Contains(MuscleGroup.Legs | MuscleGroup.Core, result);
    }

    [Fact]
    public void GetWorkoutSplits_ShouldReturnCorrectSplitsFor4Sessions()
    {
        // Arrange
        var generator = CreateGenerator();
        var methodInfo = typeof(WorkoutGenerator).GetMethod("GetWorkoutSplits", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.NotNull(methodInfo);

        // Act
        var result = methodInfo.Invoke(generator, new object[] { 4 }) as List<MuscleGroup>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Contains(MuscleGroup.Chest | MuscleGroup.Arms, result);
        Assert.Contains(MuscleGroup.Back | MuscleGroup.Shoulders, result);
        Assert.Contains(MuscleGroup.Legs, result);
        Assert.Contains(MuscleGroup.Core | MuscleGroup.Arms, result);
    }

    [Fact]
    public void GetWorkoutSplits_ShouldReturnCorrectSplitsFor5Sessions()
    {
        // Arrange
        var generator = CreateGenerator();
        var methodInfo = typeof(WorkoutGenerator).GetMethod("GetWorkoutSplits", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.NotNull(methodInfo);

        // Act
        var result = methodInfo.Invoke(generator, new object[] { 5 }) as List<MuscleGroup>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Contains(MuscleGroup.Chest, result);
        Assert.Contains(MuscleGroup.Back, result);
        Assert.Contains(MuscleGroup.Legs, result);
        Assert.Contains(MuscleGroup.Shoulders | MuscleGroup.Arms, result);
        Assert.Contains(MuscleGroup.Core, result);
    }

    [Fact]
    public void GetWorkoutSplits_ForInvalidSessions_ShouldReturnNone()
    {
        // Arrange
        var generator = CreateGenerator();
        var methodInfo = typeof(WorkoutGenerator).GetMethod("GetWorkoutSplits", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.NotNull(methodInfo);

        // Act
        var result = methodInfo.Invoke(generator, new object[] { 2 }) as List<MuscleGroup>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.Equal(MuscleGroup.None, r));
    }

    [Fact]
    public void GenerateWorkoutPlanNotes_ShouldReturnCorrectNoteForEachPhase()
    {
        // Arrange
        var generator = CreateGenerator();
        var methodInfo = typeof(WorkoutGenerator).GetMethod("GenerateWorkoutPlanNotes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.NotNull(methodInfo);

        var expectedNotes = new Dictionary<int, string>
        {
            { 1, "Foundation phase" },
            { 2, "Hypertrophy phase" },
            { 3, "Strength phase" },
            { 4, "Peaking phase" },
            { 5, "Maintenance phase" }
        };

        // Act & Assert
        foreach (var phase in expectedNotes.Keys)
        {
            var result = methodInfo.Invoke(generator, new object[] { phase }) as string;
            Assert.NotNull(result);
            Assert.Contains(expectedNotes[phase], result);
        }
    }

    // ========== Tests for CurrentId Properties ==========

    [Fact]
    public void CurrentPlanId_ShouldStartWithCorrectValue()
    {
        // Arrange & Act
        var generator1 = CreateGenerator(1, 1);
        var generator2 = CreateGenerator(50, 1);
        var generator3 = CreateGenerator(100, 1);

        // Assert
        Assert.Equal(1, generator1.CurrentPlanId);
        Assert.Equal(50, generator2.CurrentPlanId);
        Assert.Equal(100, generator3.CurrentPlanId);
    }

    [Fact]
    public void CurrentWorkoutDayId_ShouldStartWithCorrectValue()
    {
        // Arrange & Act
        var generator1 = CreateGenerator(1, 1);
        var generator2 = CreateGenerator(1, 50);
        var generator3 = CreateGenerator(1, 100);

        // Assert
        Assert.Equal(1, generator1.CurrentWorkoutDayId);
        Assert.Equal(50, generator2.CurrentWorkoutDayId);
        Assert.Equal(100, generator3.CurrentWorkoutDayId);
    }

    // ========== Integration Tests ==========

    [Fact]
    public void GenerateFullWorkoutData_ShouldCreateValidRelationships()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 2;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);
        
        var allDays = new List<WorkoutDay>();
        foreach (var plan in plans)
        {
            var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);
            allDays.AddRange(days);
        }

        // Assert
        foreach (var day in allDays)
        {
            var parentPlan = plans.FirstOrDefault(p => p.Id == day.WorkoutPlanId);
            Assert.NotNull(parentPlan);
            Assert.Equal(parentPlan.CreatedAt, day.CreatedAt);
        }
    }

    [Fact]
    public void GenerateMultiplePlans_ShouldGenerateUniquePlanIds()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 5;

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);
        var ids = plans.Select(p => p.Id).ToList();
        var distinctIds = ids.Distinct().ToList();

        // Assert
        Assert.Equal(ids.Count, distinctIds.Count);
    }

    [Fact]
    public void GenerateWorkoutDaysForMultiplePlans_ShouldGenerateUniqueDayIds()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = CreateTestUser(1);
        var planCount = 3;
        var allDays = new List<WorkoutDay>();

        // Act
        var plans = generator.GenerateWorkoutPlans(user, planCount);
        foreach (var plan in plans)
        {
            var days = generator.GenerateWorkoutDays(plan, plan.SessionsPerWeek);
            allDays.AddRange(days);
        }
        
        var ids = allDays.Select(d => d.Id).ToList();
        var distinctIds = ids.Distinct().ToList();

        // Assert
        Assert.Equal(ids.Count, distinctIds.Count);
    }

    [Fact]
    public void Exercises_ShouldHaveValidMuscleGroupFlags()
    {
        // Arrange
        var generator = CreateGenerator();

        // Act
        var exercises = generator.GenerateExercises();

        // Assert
        foreach (var exercise in exercises)
        {
            // بررسی اینکه MuscleGroup یک Flag معتبر است
            var muscleGroupValue = (int)exercise.PrimaryMuscleGroup;
            Assert.True(muscleGroupValue > 0 && muscleGroupValue <= 64);
        }
    }
}