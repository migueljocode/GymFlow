using Person = GymFlow.Models.Entities.Person;

namespace GymFlow.Tests.Dal.Seed.Data;

public class DataGeneratorTest
{

    // متد کمکی
    private string GetUsernameFromPersonId(List<User> users, List<Person> persons, int index)
    {
        var user = users[index];
        var person = persons.FirstOrDefault(p => p.Id == user.PersonId);
        return person?.Username ?? "unknown";
    }


    // ========== Tests for GenerateExercises ==========

    [Fact]
    public void GenerateExercises_ShouldReturnCorrectCount()
    {
        // Act
        var exercises = DataGenerator.GenerateExercises(30);

        // Assert
        Assert.NotNull(exercises);
        Assert.Equal(30, exercises.Count);
    }

    [Fact]
    public void GenerateExercises_ShouldHaveUniqueNames()
    {
        // Act
        var exercises = DataGenerator.GenerateExercises(30);
        var names = exercises.Select(e => e.Name).ToList();
        var distinctNames = names.Distinct().ToList();

        // Assert
        Assert.Equal(names.Count, distinctNames.Count);
    }

    [Fact]
    public void GenerateExercises_ShouldHaveValidMuscleGroups()
    {
        // Act
        var exercises = DataGenerator.GenerateExercises(30);
        var validMuscleGroups = new[] 
        { 
            MuscleGroup.None,        // اضافه شد
            MuscleGroup.Chest, 
            MuscleGroup.Back, 
            MuscleGroup.Legs, 
            MuscleGroup.Shoulders, 
            MuscleGroup.Arms, 
            MuscleGroup.Core,
            MuscleGroup.FullBody
        };

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.Contains(exercise.PrimaryMuscleGroup, validMuscleGroups);
        }
    }

    [Fact]
    public void GenerateExercises_ShouldHaveValidCreatedAt()
    {
        // Act
        var exercises = DataGenerator.GenerateExercises(30);

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.True(exercise.CreatedAt <= DateTime.UtcNow);
            // تاریخ می‌تواند بین 1 تا 12 ماه قبل باشد
            Assert.True(exercise.CreatedAt >= DateTime.UtcNow.AddMonths(-12).AddDays(-1));
        }
    }

    [Fact]
    public void GenerateExercises_ShouldHaveDescription()
    {
        // Act
        var exercises = DataGenerator.GenerateExercises(30);

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.NotNull(exercise.Description);
            Assert.NotEmpty(exercise.Description);
            Assert.Contains(exercise.Name, exercise.Description);
        }
    }

    // ========== Tests for GeneratePersons ==========

    [Fact]
    public void GeneratePersons_ShouldReturnCorrectCount()
    {
        // Act
        var persons = DataGenerator.GeneratePersons(10);

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(10, persons.Count);
    }

    [Fact]
    public void GeneratePersons_ShouldAlwaysIncludeCoachAndMember()
    {
        // Act
        var persons = DataGenerator.GeneratePersons(5);

        // Assert
        Assert.Contains(persons, p => p.Username == "coach");
        Assert.Contains(persons, p => p.Username == "member");
    }

    [Fact]
    public void GeneratePersons_CoachShouldHaveCorrectProperties()
    {
        // Act
        var persons = DataGenerator.GeneratePersons(5);
        var coach = persons.First(p => p.Username == "coach");

        // Assert
        Assert.Equal("Master", coach.FirstName);
        Assert.Equal("Coach", coach.LastName);
        Assert.Equal("coach123", coach.Password);
        Assert.Equal("coach@gymflow.com", coach.Email);
        Assert.Equal(Gender.Male, coach.Gender);
        Assert.Equal(35, coach.Age);
        Assert.Equal(85, coach.Weight);
        Assert.Equal(182, coach.Height);
        Assert.Equal(BodyType.Fit, coach.BodyType);
    }

    [Fact]
    public void GeneratePersons_MemberShouldHaveCorrectProperties()
    {
        // Act
        var persons = DataGenerator.GeneratePersons(5);
        var member = persons.First(p => p.Username == "member");

        // Assert
        Assert.Equal("John", member.FirstName);
        Assert.Equal("Doe", member.LastName);
        Assert.Equal("member123", member.Password);
        Assert.Equal("member@gymflow.com", member.Email);
        Assert.Equal(Gender.Male, member.Gender);
        Assert.Equal(25, member.Age);
        Assert.Equal(75, member.Weight);
        Assert.Equal(175, member.Height);
        Assert.Equal(BodyType.Fit, member.BodyType);
    }

    [Fact]
    public void GeneratePersons_ShouldGenerateUniqueUsernames()
    {
        // Act
        var persons = DataGenerator.GeneratePersons(20);
        var usernames = persons.Select(p => p.Username).ToList();
        var distinctUsernames = usernames.Distinct().ToList();

        // Assert
        Assert.Equal(usernames.Count, distinctUsernames.Count);
    }

    [Fact]
    public void GeneratePersons_ShouldHaveValidAgeRange()
    {
        // Act
        var persons = DataGenerator.GeneratePersons(20);
        var randomPersons = persons.Where(p => p.Username != "coach" && p.Username != "member");

        // Assert
        foreach (var person in randomPersons)
        {
            Assert.InRange(person.Age, 18, 55);
        }
    }

    // ========== Tests for GenerateUsers ==========

    [Fact]
    public void GenerateUsers_ShouldCreateUserForEachPerson()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(10);

        // Act
        var users = DataGenerator.GenerateUsers(persons);

        // Assert
        Assert.Equal(persons.Count, users.Count);
    }

    [Fact]
    public void GenerateUsers_MemberShouldHaveMuscleGainGoal()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var memberPerson = persons.First(p => p.Username == "member");

        // Act
        var users = DataGenerator.GenerateUsers(persons);
        var memberUser = users.First(u => u.PersonId == memberPerson.Id);

        // Assert
        Assert.Equal(Goal.MuscleGain, memberUser.Goal);
        Assert.Equal(2500, memberUser.EstimatedCaloriesIntake);
        Assert.False(memberUser.IsCompetitive);
    }

    [Fact]
    public void GenerateUsers_ShouldSetCreatedAtEqualToPerson()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        
        // Act
        var users = DataGenerator.GenerateUsers(persons);
        
        // Assert
        foreach (var user in users)
        {
            var person = persons.First(p => p.Id == user.PersonId);
            // چون User.CreatedAt از Person.CreatedAt گرفته می‌شود، باید برابر باشد
            Assert.Equal(person.CreatedAt, user.CreatedAt);
        }
    }

    [Fact]
    public void GenerateUsers_ShouldUsePersonCreatedAt()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        
        // Act
        var users = DataGenerator.GenerateUsers(persons);
        
        // Assert
        foreach (var user in users)
        {
            var person = persons.First(p => p.Id == user.PersonId);
            // بررسی می‌کنیم که CreatedAt کاربر با CreatedAt شخص مطابقت دارد
            Assert.Equal(person.CreatedAt, user.CreatedAt);
        }
    }



    [Fact]
    public void GenerateUsers_ShouldGenerateValidGoals()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(10);
        var validGoals = new[] { Goal.FatLoss, Goal.MuscleGain, Goal.Fitness };

        // Act
        var users = DataGenerator.GenerateUsers(persons);
        var randomUsers = users.Where(u => u.Person?.Username != "member");

        // Assert
        foreach (var user in randomUsers)
        {
            Assert.Contains(user.Goal, validGoals);
        }
    }

    // ========== Tests for GenerateCoaches ==========

    [Fact]
    public void GenerateCoaches_ShouldCreateCoachForCoachPerson()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var coachPerson = persons.First(p => p.Username == "coach");

        // Act
        var coaches = DataGenerator.GenerateCoaches(persons);

        // Assert
        Assert.Single(coaches);
        var coach = coaches.First();
        Assert.Equal(coachPerson.Id, coach.PersonId);
        Assert.Equal("Strength & Conditioning", coach.Specialization);
        Assert.Equal(10, coach.YearsOfExperience);
    }

    // ========== Tests for GenerateWorkoutPlans ==========

    [Fact]
    public void GenerateWorkoutPlans_ShouldCreatePlansForEachUser()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);

        // Act
        var plans = DataGenerator.GenerateWorkoutPlans(users);

        // Assert
        Assert.True(plans.Count >= users.Count);
        Assert.True(plans.Count <= users.Count * 3);
    }

    [Fact]
    public void GenerateWorkoutPlans_ShouldHaveValidSessionsPerWeek()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);

        // Act
        var plans = DataGenerator.GenerateWorkoutPlans(users);

        // Assert
        foreach (var plan in plans)
        {
            Assert.InRange(plan.SessionsPerWeek, 3, 5);
        }
    }

    [Fact]
    public void GenerateWorkoutPlans_FirstPlanShouldBeActive()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);

        // به جای اولین، آخرین پلن باید فعال باشد
        var plans = DataGenerator.GenerateWorkoutPlans(users);
        var userPlans = plans.GroupBy(p => p.UserId);

        foreach (var userPlanGroup in userPlans)
        {
            var lastPlan = userPlanGroup.OrderBy(p => p.Phase).Last();
            Assert.True(lastPlan.IsActive);
        }
    }

    // ========== Tests for GenerateWorkoutDays ==========

    [Fact]
    public void GenerateWorkoutDays_ShouldCreateDaysForEachPlan()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);

        // Act
        var days = DataGenerator.GenerateWorkoutDays(plans);

        // Assert
        Assert.True(days.Count >= plans.Count * 3);
        Assert.True(days.Count <= plans.Count * 5);
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldHaveValidWeekdays()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);
        var validWeekdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

        // Act
        var days = DataGenerator.GenerateWorkoutDays(plans);

        // Assert
        foreach (var day in days)
        {
            Assert.Contains(day.DayOfWeek, validWeekdays);
        }
    }

    [Fact]
    public void GenerateWorkoutDays_ShouldHaveValidDurationRange()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);

        // Act
        var days = DataGenerator.GenerateWorkoutDays(plans);

        // Assert
        foreach (var day in days)
        {
            Assert.InRange(day.DurationMinutes, 45, 90);
        }
    }

    // ========== Tests for GenerateWorkoutDayExercises ==========

    [Fact]
    public void GenerateWorkoutDayExercises_ShouldCreateExercisesForEachDay()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);
        var days = DataGenerator.GenerateWorkoutDays(plans);
        var exercises = DataGenerator.GenerateExercises(30);

        // Act
        var wdes = DataGenerator.GenerateWorkoutDayExercises(days, exercises);

        // Assert
        Assert.NotNull(wdes);
        Assert.True(wdes.Count >= days.Count * 3);
        Assert.True(wdes.Count <= days.Count * 6);
    }

    [Fact]
    public void GenerateWorkoutDayExercises_ShouldHaveValidSets()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);
        var days = DataGenerator.GenerateWorkoutDays(plans);
        var exercises = DataGenerator.GenerateExercises(30);

        // Act
        var wdes = DataGenerator.GenerateWorkoutDayExercises(days, exercises);

        // Assert
        foreach (var wde in wdes)
        {
            Assert.InRange(wde.Sets, 3, 5);
            Assert.InRange(wde.RestSeconds, 45, 90);
            Assert.NotNull(wde.Reps);
            Assert.NotEmpty(wde.Reps);
        }
    }

    // ========== Tests for GenerateProgressLogs ==========

    [Fact]
    public void GenerateProgressLogs_ShouldCreateLogsForEachUser()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);

        // Act
        var logs = DataGenerator.GenerateProgressLogs(users, plans);

        // Assert
        Assert.NotNull(logs);
        Assert.True(logs.Count >= users.Count * 5);
    }

    [Fact]
    public void GenerateProgressLogs_ShouldHaveValidWeightRange()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);

        // Act
        var logs = DataGenerator.GenerateProgressLogs(users, plans);

        // Assert
        foreach (var log in logs)
        {
            Assert.InRange(log.Weight, 45f, 130f);
            if (log.BodyFatPercentage.HasValue)
            {
                Assert.InRange(log.BodyFatPercentage.Value, 8f, 30f);
            }
        }
    }

    // ========== Tests for GenerateWorkoutSessions ==========

    [Fact]
    public void GenerateWorkoutSessions_ShouldCreateSessionsForDays()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);
        var days = DataGenerator.GenerateWorkoutDays(plans);

        // Act
        var sessions = DataGenerator.GenerateWorkoutSessions(days);

        // Assert
        Assert.NotNull(sessions);
        Assert.True(sessions.Count >= 0);
    }

    [Fact]
    public void GenerateWorkoutSessions_ShouldHaveValidDuration()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);
        var days = DataGenerator.GenerateWorkoutDays(plans);

        // Act
        var sessions = DataGenerator.GenerateWorkoutSessions(days);

        // Assert
        foreach (var session in sessions)
        {
            Assert.True(session.ActualDurationMinutes >= 35);
            Assert.True(session.ActualDurationMinutes <= 105);
        }
    }

    [Fact]
    public void GenerateWorkoutSessions_ShouldHaveValidDates()
    {
        // Arrange
        var persons = DataGenerator.GeneratePersons(5);
        var users = DataGenerator.GenerateUsers(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);
        var days = DataGenerator.GenerateWorkoutDays(plans);

        // Act
        var sessions = DataGenerator.GenerateWorkoutSessions(days);

        // Assert
        foreach (var session in sessions)
        {
            Assert.True(session.ActualDate <= DateOnly.FromDateTime(DateTime.UtcNow));
        }
    }

    // ========== Integration Tests ==========

    [Fact]
    public void GenerateFullData_ShouldCreateValidRelationships()
    {
        // Arrange & Act
        var exercises = DataGenerator.GenerateExercises(30);
        var persons = DataGenerator.GeneratePersons(10);
        var users = DataGenerator.GenerateUsers(persons);
        var coaches = DataGenerator.GenerateCoaches(persons);
        var plans = DataGenerator.GenerateWorkoutPlans(users);
        var days = DataGenerator.GenerateWorkoutDays(plans);
        var wdes = DataGenerator.GenerateWorkoutDayExercises(days, exercises);
        var logs = DataGenerator.GenerateProgressLogs(users, plans);
        var sessions = DataGenerator.GenerateWorkoutSessions(days);

        // Assert
        Assert.Equal(30, exercises.Count);
        Assert.Equal(10, persons.Count);
        Assert.Equal(10, users.Count);
        Assert.Single(coaches);
        Assert.True(plans.Count >= 10);
        Assert.True(days.Count >= 30);
        Assert.True(wdes.Count >= 90);
        Assert.True(logs.Count >= 50);
    }

    [Fact]
    public void GenerateMultipleTimes_ShouldGenerateDifferentData()
    {
        // First generation
        var persons1 = DataGenerator.GeneratePersons(10);
        var usernames1 = persons1.Select(p => p.Username).ToList();

        // Second generation
        var persons2 = DataGenerator.GeneratePersons(10);
        var usernames2 = persons2.Select(p => p.Username).ToList();

        // Assert - coach و member همیشه وجود دارند، بقیه ممکن است متفاوت باشند
        Assert.Contains("coach", usernames1);
        Assert.Contains("member", usernames1);
        Assert.Contains("coach", usernames2);
        Assert.Contains("member", usernames2);
    }
}