using Xunit;
using GymFlow.Dal.Seed.Data;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;
using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Tests.Dal.Seed.Data;

public class SeedDataGeneratorTest
{
    private readonly SeedOptions _options;

    public SeedDataGeneratorTest()
    {
        _options = SeedProfiles.Lightweight;
        _options.RandomSeed = 42;
    }

    // ========== Helper Methods ==========

    private SeedDataGenerator CreateGenerator()
    {
        return new SeedDataGenerator(_options);
    }

    // ========== Tests for GenerateExercises ==========

    [Fact]
    public void GenerateExercises_ShouldReturnCorrectCount()
    {
        // Arrange
        var generator = CreateGenerator();

        // Act
        var exercises = generator.GenerateExercises();

        // Assert
        Assert.NotNull(exercises);
        Assert.Equal(30, exercises.Count);
    }

    [Fact]
    public void GenerateExercises_ShouldHaveUniqueNames()
    {
        // Arrange
        var generator = CreateGenerator();

        // Act
        var exercises = generator.GenerateExercises();
        var names = exercises.Select(e => e.Name).ToList();
        var distinctNames = names.Distinct().ToList();

        // Assert
        Assert.Equal(names.Count, distinctNames.Count);
    }

    [Fact]
    public void GenerateExercises_ShouldHaveValidCreatedAt()
    {
        // Arrange
        var generator = CreateGenerator();

        // Act
        var exercises = generator.GenerateExercises();

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.True(exercise.CreatedAt <= DateTime.UtcNow);
            Assert.True(exercise.CreatedAt >= DateTime.UtcNow.AddMonths(-12).AddDays(-1));
        }
    }

    // ========== Tests for GeneratePersons ==========

    [Fact]
    public void GeneratePersons_ShouldReturnCorrectCount()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 5;

        // Act
        var persons = generator.GeneratePersons();

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(2 + _options.UserCount, persons.Count); // coach + member + UserCount
    }

    [Fact]
    public void GeneratePersons_ShouldAlwaysIncludeCoachAndMember()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;

        // Act
        var persons = generator.GeneratePersons();

        // Assert
        Assert.Contains(persons, p => p.Username == "coach");
        Assert.Contains(persons, p => p.Username == "member");
    }

    [Fact]
    public void GeneratePersons_CoachShouldHaveCorrectProperties()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;

        // Act
        var persons = generator.GeneratePersons();
        var coach = persons.First(p => p.Username == "coach");

        // Assert
        Assert.Equal("Master", coach.FirstName);
        Assert.Equal("Coach", coach.LastName);
        Assert.Equal("coach123", coach.Password);
        Assert.Equal("coach@gymflow.com", coach.Email);
        Assert.Equal(Gender.Male, coach.Gender);
        Assert.Equal(35, coach.Age);
        Assert.Equal(85.5f, coach.Weight);
        Assert.Equal(182f, coach.Height);
        Assert.Equal(BodyType.Fit, coach.BodyType);
    }

    [Fact]
    public void GeneratePersons_MemberShouldHaveCorrectProperties()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;

        // Act
        var persons = generator.GeneratePersons();
        var member = persons.First(p => p.Username == "member");

        // Assert
        Assert.Equal("John", member.FirstName);
        Assert.Equal("Doe", member.LastName);
        Assert.Equal("member123", member.Password);
        Assert.Equal("member@gymflow.com", member.Email);
        Assert.Equal(Gender.Male, member.Gender);
        Assert.Equal(25, member.Age);
        Assert.Equal(75.0f, member.Weight);
        Assert.Equal(175f, member.Height);
        Assert.Equal(BodyType.Fit, member.BodyType);
    }

    [Fact]
    public void GeneratePersons_ShouldHaveValidAgeRange()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 20;

        // Act
        var persons = generator.GeneratePersons();
        var randomPersons = persons.Where(p => p.Username != "coach" && p.Username != "member");

        // Assert
        foreach (var person in randomPersons)
        {
            Assert.InRange(person.Age, 18, 55);
        }
    }

    // ========== Tests for GenerateUsers ==========

    [Fact]
    public void GenerateUsers_ShouldCreateUsersForAllPersons()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 5;
        var persons = generator.GeneratePersons();

        // Act
        var users = generator.GenerateUsers(persons);

        // Assert
        Assert.NotNull(users);
        Assert.Equal(persons.Count - 1, users.Count); // coach ندارد
    }

    [Fact]
    public void GenerateUsers_MemberShouldHaveMuscleGainGoal()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;
        var persons = generator.GeneratePersons();
        var memberPerson = persons.First(p => p.Username == "member");

        // Act
        var users = generator.GenerateUsers(persons);
        var memberUser = users.First(u => u.PersonId == memberPerson.Id);

        // Assert
        Assert.Equal(Goal.MuscleGain, memberUser.Goal);
        Assert.Equal(2500, memberUser.EstimatedCaloriesIntake);
    }

    [Fact]
    public void GenerateUsers_ShouldSetCreatedAtFromPerson()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 5;
        var persons = generator.GeneratePersons();

        // Act
        var users = generator.GenerateUsers(persons);

        // Assert - با استفاده از PersonId مطابقت دهیم
        foreach (var user in users)
        {
            var person = persons.First(p => p.Id == user.PersonId);
            // تاریخ‌ها باید برابر باشند
            Assert.Equal(person.CreatedAt, user.CreatedAt);
        }
    }

    // ========== Tests for GenerateCoaches ==========

    [Fact]
    public void GenerateCoaches_ShouldCreateCoachForCoachPerson()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;
        var persons = generator.GeneratePersons();
        var coachPerson = persons.First(p => p.Username == "coach");

        // Act
        var coaches = generator.GenerateCoaches(persons);

        // Assert
        Assert.Single(coaches);
        var coach = coaches.First();
        Assert.Equal(coachPerson.Id, coach.PersonId);
        Assert.Equal("Strength & Conditioning", coach.Specialization);
        Assert.Equal(10, coach.YearsOfExperience);
    }

    // ========== Tests for GenerateWorkoutData ==========

    [Fact]
    public void GenerateWorkoutData_ShouldCreateValidRelationships()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;
        _options.MinWorkoutPlansPerUser = 1;
        _options.MaxWorkoutPlansPerUser = 2;
        
        var persons = generator.GeneratePersons();
        var users = generator.GenerateUsers(persons);
        var exercises = generator.GenerateExercises();

        // Act
        var (plans, days, wdes, logs, sessions) = generator.GenerateWorkoutData(users, exercises);

        // Assert
        Assert.NotNull(plans);
        Assert.NotNull(days);
        Assert.NotNull(wdes);
        Assert.NotNull(logs);
        Assert.NotNull(sessions);

        Assert.True(plans.Count > 0);
        Assert.True(days.Count > 0);
        Assert.True(wdes.Count > 0);
    }

    [Fact]
    public void GenerateWorkoutData_PlansShouldHaveCorrectPhases()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;
        _options.MinWorkoutPlansPerUser = 2;
        _options.MaxWorkoutPlansPerUser = 3;
        
        var persons = generator.GeneratePersons();
        var users = generator.GenerateUsers(persons);
        var exercises = generator.GenerateExercises();

        // Act
        var (plans, _, _, _, _) = generator.GenerateWorkoutData(users, exercises);
        
        var userPlans = plans.GroupBy(p => p.UserId);

        // Assert - هر کاربر باید phases متوالی داشته باشد
        foreach (var userPlanGroup in userPlans)
        {
            var phases = userPlanGroup.Select(p => p.Phase).OrderBy(p => p).ToList();
            for (int i = 0; i < phases.Count; i++)
            {
                Assert.Equal(i + 1, phases[i]);
            }
        }
    }

    [Fact]
    public void GenerateWorkoutData_LastPlanShouldBeActive()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;
        _options.MinWorkoutPlansPerUser = 2;
        _options.MaxWorkoutPlansPerUser = 3;
        
        var persons = generator.GeneratePersons();
        var users = generator.GenerateUsers(persons);
        var exercises = generator.GenerateExercises();

        // Act
        var (plans, _, _, _, _) = generator.GenerateWorkoutData(users, exercises);
        
        var userPlans = plans.GroupBy(p => p.UserId);

        // Assert - فقط آخرین پلن باید فعال باشد
        foreach (var userPlanGroup in userPlans)
        {
            var sortedPlans = userPlanGroup.OrderBy(p => p.Phase).ToList();
            for (int i = 0; i < sortedPlans.Count; i++)
            {
                if (i == sortedPlans.Count - 1)
                {
                    Assert.True(sortedPlans[i].IsActive, $"Plan phase {sortedPlans[i].Phase} should be active");
                }
                else
                {
                    Assert.False(sortedPlans[i].IsActive, $"Plan phase {sortedPlans[i].Phase} should not be active");
                }
            }
        }
    }

    [Fact]
    public void GenerateWorkoutData_WorkoutDaysShouldHaveValidWeekdays()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;
        _options.MinWorkoutDaysPerPlan = 3;
        _options.MaxWorkoutDaysPerPlan = 5;
        
        var persons = generator.GeneratePersons();
        var users = generator.GenerateUsers(persons);
        var exercises = generator.GenerateExercises();
        var validWeekdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
                                     DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };

        // Act
        var (_, days, _, _, _) = generator.GenerateWorkoutData(users, exercises);

        // Assert
        foreach (var day in days)
        {
            Assert.Contains(day.DayOfWeek, validWeekdays);
        }
    }

    [Fact]
    public void GenerateWorkoutData_WorkoutDaysShouldHaveNoDuplicateWeekdays()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;
        
        var persons = generator.GeneratePersons();
        var users = generator.GenerateUsers(persons);
        var exercises = generator.GenerateExercises();

        // Act
        var (plans, days, _, _, _) = generator.GenerateWorkoutData(users, exercises);
        
        var planDays = days.GroupBy(d => d.WorkoutPlanId);

        // Assert
        foreach (var planDayGroup in planDays)
        {
            var weekdays = planDayGroup.Select(d => d.DayOfWeek).ToList();
            var distinctWeekdays = weekdays.Distinct().ToList();
            Assert.Equal(weekdays.Count, distinctWeekdays.Count);
        }
    }

    [Fact]
    public void GenerateWorkoutData_ShouldHaveValidProgressLogs()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;
        
        var persons = generator.GeneratePersons();
        var users = generator.GenerateUsers(persons);
        var exercises = generator.GenerateExercises();

        // Act
        var (plans, _, _, logs, _) = generator.GenerateWorkoutData(users, exercises);

        // Assert
        foreach (var log in logs)
        {
            Assert.InRange(log.Weight, 45f, 130f);
            // تاریخ لاگ نباید خیلی در آینده باشد (حداکثر امروز)
            Assert.True(log.LogDate <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7), 
                $"Log date {log.LogDate} is too far in the future");
        }
    }

    [Fact]
    public void GenerateWorkoutData_ShouldHaveValidWorkoutSessions()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 3;
        
        var persons = generator.GeneratePersons();
        var users = generator.GenerateUsers(persons);
        var exercises = generator.GenerateExercises();

        // Act
        var (_, _, _, _, sessions) = generator.GenerateWorkoutData(users, exercises);

        // Assert
        foreach (var session in sessions)
        {
            Assert.True(session.ActualDate <= DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.True(session.ActualDurationMinutes >= 35);
            Assert.True(session.ActualDurationMinutes <= 105);
        }
    }

    // ========== Tests for DetermineBodyType (via GeneratePersons) ==========

    [Fact]
    public void DetermineBodyType_ShouldReturnCorrectBodyTypeBasedOnBMI()
    {
        // Arrange
        var generator = CreateGenerator();
        
        // با استفاده از Reflection متد خصوصی را تست می‌کنیم
        var methodInfo = typeof(SeedDataGenerator).GetMethod("DetermineBodyType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.NotNull(methodInfo);

        // Act & Assert
        var testCases = new[]
        {
            new { Weight = 45f, Height = 170f, Expected = BodyType.LeanMuscular }, // BMI = 15.6
            new { Weight = 60f, Height = 170f, Expected = BodyType.Fit },          // BMI = 20.8
            new { Weight = 80f, Height = 170f, Expected = BodyType.Overweight },    // BMI = 27.7
            new { Weight = 100f, Height = 170f, Expected = BodyType.Obese }         // BMI = 34.6
        };
        
        foreach (var testCase in testCases)
        {
            var result = methodInfo.Invoke(generator, new object[] { testCase.Weight, testCase.Height });
            Assert.Equal(testCase.Expected, result);
        }
    }

    // ========== Integration Tests ==========

    [Fact]
    public void FullDataGeneration_ShouldCreateAllEntities()
    {
        // Arrange
        var generator = CreateGenerator();
        _options.UserCount = 5;
        _options.MinWorkoutPlansPerUser = 1;
        _options.MaxWorkoutPlansPerUser = 2;
        _options.MinWorkoutDaysPerPlan = 3;
        _options.MaxWorkoutDaysPerPlan = 4;
        _options.MinExercisesPerDay = 3;
        _options.MaxExercisesPerDay = 5;

        // Act
        var exercises = generator.GenerateExercises();
        var persons = generator.GeneratePersons();
        var users = generator.GenerateUsers(persons);
        var coaches = generator.GenerateCoaches(persons);
        var (plans, days, wdes, logs, sessions) = generator.GenerateWorkoutData(users, exercises);

        // Assert
        Assert.Equal(30, exercises.Count);
        Assert.Equal(2 + _options.UserCount, persons.Count);
        Assert.Equal(persons.Count - 1, users.Count);
        Assert.Single(coaches);
        Assert.True(plans.Count >= users.Count);
        Assert.True(days.Count >= plans.Count * _options.MinWorkoutDaysPerPlan);
        Assert.True(wdes.Count >= days.Count * _options.MinExercisesPerDay);
        Assert.True(logs.Count >= users.Count * 3);
        Assert.True(sessions.Count >= days.Count * 4);
    }

    [Fact]
    public void SameSeed_ShouldGenerateSameData()
    {
        // Arrange
        var options1 = SeedProfiles.Lightweight;
        options1.RandomSeed = 123;
        var options2 = SeedProfiles.Lightweight;
        options2.RandomSeed = 123;
        
        var generator1 = new SeedDataGenerator(options1);
        var generator2 = new SeedDataGenerator(options2);

        // Act
        var exercises1 = generator1.GenerateExercises();
        var exercises2 = generator2.GenerateExercises();
        
        var persons1 = generator1.GeneratePersons();
        var persons2 = generator2.GeneratePersons();

        // Assert
        Assert.Equal(exercises1.Count, exercises2.Count);
        for (int i = 0; i < exercises1.Count; i++)
        {
            Assert.Equal(exercises1[i].Name, exercises2[i].Name);
        }
        
        Assert.Equal(persons1.Count, persons2.Count);
    }
}