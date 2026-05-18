using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Tests.Dal.Seed.Data;

public class SeedDataResultTest
{
    [Fact]
    public void SeedDataResult_DefaultConstructor_ShouldInitializeAllCollections()
    {
        // Act
        var result = new SeedDataResult();

        // Assert
        Assert.NotNull(result.Exercises);
        Assert.NotNull(result.Persons);
        Assert.NotNull(result.Users);
        Assert.NotNull(result.Coaches);
        Assert.NotNull(result.WorkoutPlans);
        Assert.NotNull(result.WorkoutDays);
        Assert.NotNull(result.WorkoutDayExercises);
        Assert.NotNull(result.ProgressLogs);
        Assert.NotNull(result.WorkoutSessions);
    }

    [Fact]
    public void SeedDataResult_ShouldInitializeWithEmptyCollections()
    {
        // Act
        var result = new SeedDataResult();

        // Assert
        Assert.Empty(result.Exercises);
        Assert.Empty(result.Persons);
        Assert.Empty(result.Users);
        Assert.Empty(result.Coaches);
        Assert.Empty(result.WorkoutPlans);
        Assert.Empty(result.WorkoutDays);
        Assert.Empty(result.WorkoutDayExercises);
        Assert.Empty(result.ProgressLogs);
        Assert.Empty(result.WorkoutSessions);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingExercises()
    {
        // Arrange
        var result = new SeedDataResult();
        var exercise = new Exercise
        {
            Id = 1,
            Name = "Bench Press",
            PrimaryMuscleGroup = MuscleGroup.Chest,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        result.Exercises.Add(exercise);

        // Assert
        Assert.Single(result.Exercises);
        Assert.Equal(exercise.Name, result.Exercises[0].Name);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingPersons()
    {
        // Arrange
        var result = new SeedDataResult();
        var person = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        result.Persons.Add(person);

        // Assert
        Assert.Single(result.Persons);
        Assert.Equal(person.Username, result.Persons[0].Username);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingUsers()
    {
        // Arrange
        var result = new SeedDataResult();
        var user = new User
        {
            Id = 1,
            PersonId = 1,
            Goal = Goal.Fitness,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        result.Users.Add(user);

        // Assert
        Assert.Single(result.Users);
        Assert.Equal(user.Goal, result.Users[0].Goal);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingCoaches()
    {
        // Arrange
        var result = new SeedDataResult();
        var coach = new Coach
        {
            Id = 1,
            PersonId = 1,
            Specialization = "Strength Training",
            YearsOfExperience = 10,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        result.Coaches.Add(coach);

        // Assert
        Assert.Single(result.Coaches);
        Assert.Equal(coach.Specialization, result.Coaches[0].Specialization);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingWorkoutPlans()
    {
        // Arrange
        var result = new SeedDataResult();
        var plan = new WorkoutPlan
        {
            Id = 1,
            UserId = 1,
            Phase = 1,
            SessionsPerWeek = 3,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        result.WorkoutPlans.Add(plan);

        // Assert
        Assert.Single(result.WorkoutPlans);
        Assert.Equal(plan.Phase, result.WorkoutPlans[0].Phase);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingWorkoutDays()
    {
        // Arrange
        var result = new SeedDataResult();
        var day = new WorkoutDay
        {
            Id = 1,
            WorkoutPlanId = 1,
            DayOfWeek = DayOfWeek.Monday,
            TargetMuscles = MuscleGroup.Chest,
            DurationMinutes = 60,
            Intensity = Intensity.Medium,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        result.WorkoutDays.Add(day);

        // Assert
        Assert.Single(result.WorkoutDays);
        Assert.Equal(day.DayOfWeek, result.WorkoutDays[0].DayOfWeek);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingWorkoutDayExercises()
    {
        // Arrange
        var result = new SeedDataResult();
        var wde = new WorkoutDayExercise
        {
            Id = 1,
            WorkoutDayId = 1,
            ExerciseId = 1,
            Sets = 3,
            Reps = "10,10,8",
            RestSeconds = 60,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        result.WorkoutDayExercises.Add(wde);

        // Assert
        Assert.Single(result.WorkoutDayExercises);
        Assert.Equal(wde.Sets, result.WorkoutDayExercises[0].Sets);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingProgressLogs()
    {
        // Arrange
        var result = new SeedDataResult();
        var log = new ProgressLog
        {
            Id = 1,
            UserId = 1,
            WorkoutPlanId = 1,
            LogDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Weight = 75.5f,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        result.ProgressLogs.Add(log);

        // Assert
        Assert.Single(result.ProgressLogs);
        Assert.Equal(log.Weight, result.ProgressLogs[0].Weight);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingWorkoutSessions()
    {
        // Arrange
        var result = new SeedDataResult();
        var session = new WorkoutSession
        {
            Id = 1,
            WorkoutDayId = 1,
            ActualDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ActualDurationMinutes = 60,
            Feeling = "Great!",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        result.WorkoutSessions.Add(session);

        // Assert
        Assert.Single(result.WorkoutSessions);
        Assert.Equal(session.Feeling, result.WorkoutSessions[0].Feeling);
    }

    [Fact]
    public void SeedDataResult_ShouldAllowAddingMultipleItems()
    {
        // Arrange
        var result = new SeedDataResult();

        // Act
        for (int i = 1; i <= 5; i++)
        {
            result.Exercises.Add(new Exercise { Id = i, Name = $"Exercise{i}", CreatedAt = DateTime.UtcNow });
            result.Persons.Add(new Person { Id = i, Username = $"person{i}", CreatedAt = DateTime.UtcNow });
            result.Users.Add(new User { Id = i, PersonId = i, Goal = Goal.Fitness, CreatedAt = DateTime.UtcNow });
        }

        // Assert
        Assert.Equal(5, result.Exercises.Count);
        Assert.Equal(5, result.Persons.Count);
        Assert.Equal(5, result.Users.Count);
    }

    [Fact]
    public void SeedDataResult_ShouldBeAbleToClearCollections()
    {
        // Arrange
        var result = new SeedDataResult();
        result.Exercises.Add(new Exercise { Id = 1, Name = "Test", CreatedAt = DateTime.UtcNow });
        result.Persons.Add(new Person { Id = 1, Username = "test", CreatedAt = DateTime.UtcNow });

        // Act
        result.Exercises.Clear();
        result.Persons.Clear();

        // Assert
        Assert.Empty(result.Exercises);
        Assert.Empty(result.Persons);
    }

    [Fact]
    public void SeedDataResult_Collections_ShouldBeIndependent()
    {
        // Arrange
        var result = new SeedDataResult();
        
        // Act
        result.Exercises.Add(new Exercise { Id = 1, Name = "Exercise1", CreatedAt = DateTime.UtcNow });
        result.Persons.Add(new Person { Id = 1, Username = "Person1", CreatedAt = DateTime.UtcNow });

        // Assert
        Assert.Single(result.Exercises);
        Assert.Single(result.Persons);
        Assert.Empty(result.Users);
        Assert.Empty(result.Coaches);
        Assert.Empty(result.WorkoutPlans);
        Assert.Empty(result.WorkoutDays);
        Assert.Empty(result.WorkoutDayExercises);
        Assert.Empty(result.ProgressLogs);
        Assert.Empty(result.WorkoutSessions);
    }

    [Fact]
    public void SeedDataResult_ShouldPreserveDataIntegrity()
    {
        // Arrange
        var result = new SeedDataResult();
        var expectedName = "Squat";
        var expectedUsername = "testuser";
        
        // Act
        result.Exercises.Add(new Exercise { Id = 1, Name = expectedName, CreatedAt = DateTime.UtcNow });
        result.Persons.Add(new Person { Id = 1, Username = expectedUsername, CreatedAt = DateTime.UtcNow });
        
        result.WorkoutPlans.Add(new WorkoutPlan 
        { 
            Id = 1, 
            UserId = 1, 
            Phase = 1, 
            SessionsPerWeek = 3, 
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow 
        });

        // Assert
        Assert.Equal(expectedName, result.Exercises[0].Name);
        Assert.Equal(expectedUsername, result.Persons[0].Username);
        Assert.Equal(1, result.WorkoutPlans[0].UserId);
    }
}