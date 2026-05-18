namespace GymFlow.Tests.Dal.Seed.Data;

public class HardCodedDataTest
{
    // ========== Tests for GetExercises ==========

    [Fact]
    public void GetExercises_ShouldReturnCorrectCount()
    {
        // Act
        var exercises = HardCodedData.GetExercises();

        // Assert
        Assert.NotNull(exercises);
        Assert.Equal(10, exercises.Count);
    }

    [Fact]
    public void GetExercises_ShouldHaveValidMuscleGroups()
    {
        // Act
        var exercises = HardCodedData.GetExercises();
        var validMuscleGroups = new[] 
        { 
            MuscleGroup.Chest, MuscleGroup.Back, MuscleGroup.Legs, 
            MuscleGroup.Shoulders, MuscleGroup.Arms, MuscleGroup.Core 
        };

        // Assert
        foreach (var exercise in exercises)
        {
            Assert.Contains(exercise.PrimaryMuscleGroup, validMuscleGroups);
        }
    }

    [Fact]
    public void GetExercises_ShouldHaveNonEmptyNames()
    {
        // Act
        var exercises = HardCodedData.GetExercises();
        
        // Assert
        Assert.NotNull(exercises);          // <-- اضافه شد
        Assert.NotEmpty(exercises);
        
        foreach (var exercise in exercises)
        {
            Assert.NotNull(exercise.Name);
            Assert.NotEmpty(exercise.Name);
            Assert.NotNull(exercise.Description);
            Assert.NotEmpty(exercise.Description);
        }
    }

    [Fact]
    public void GetExercises_ShouldHaveBenchPress()
    {
        // Act
        var exercises = HardCodedData.GetExercises();
        var benchPress = exercises.FirstOrDefault(e => e.Name == "Bench Press");

        // Assert
        Assert.NotNull(benchPress);
        Assert.Equal(MuscleGroup.Chest, benchPress.PrimaryMuscleGroup);
        Assert.Equal("Chest exercise", benchPress.Description);
    }

    [Fact]
    public void GetExercises_ShouldHaveDeadlift()
    {
        // Act
        var exercises = HardCodedData.GetExercises();
        var deadlift = exercises.FirstOrDefault(e => e.Name == "Deadlift");

        // Assert
        Assert.NotNull(deadlift);
        Assert.Equal(MuscleGroup.Back, deadlift.PrimaryMuscleGroup);
    }

    // ========== Tests for GetPersons ==========

    [Fact]
    public void GetPersons_ShouldReturnCorrectCount()
    {
        // Act
        var persons = HardCodedData.GetPersons();

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(3, persons.Count);
    }

    [Fact]
    public void GetPersons_ShouldHaveCoach()
    {
        // Act
        var persons = HardCodedData.GetPersons();
        var coach = persons.FirstOrDefault(p => p.Username == "coach");

        // Assert
        Assert.NotNull(coach);
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
    public void GetPersons_ShouldHaveMember()
    {
        // Act
        var persons = HardCodedData.GetPersons();
        var member = persons.FirstOrDefault(p => p.Username == "member");

        // Assert
        Assert.NotNull(member);
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
    public void GetPersons_ShouldHaveJaneSmith()
    {
        // Act
        var persons = HardCodedData.GetPersons();
        var jane = persons.FirstOrDefault(p => p.Username == "janesmith");

        // Assert
        Assert.NotNull(jane);
        Assert.Equal("Jane", jane.FirstName);
        Assert.Equal("Smith", jane.LastName);
        Assert.Equal("password123", jane.Password);
        Assert.Equal("jane@test.com", jane.Email);
        Assert.Equal(Gender.Female, jane.Gender);
        Assert.Equal(28, jane.Age);
        Assert.Equal(65, jane.Weight);
        Assert.Equal(165, jane.Height);
        Assert.Equal(BodyType.Fit, jane.BodyType);
    }

    // ========== Tests for GetUsers ==========

    [Fact]
    public void GetUsers_ShouldReturnCorrectCount()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();

        // Act
        var users = HardCodedData.GetUsers(persons);

        // Assert
        Assert.NotNull(users);
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public void GetUsers_ShouldHaveMemberUser()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var memberPerson = persons.First(p => p.Username == "member");

        // Act
        var users = HardCodedData.GetUsers(persons);
        var memberUser = users.FirstOrDefault(u => u.PersonId == memberPerson.Id);

        // Assert
        Assert.NotNull(memberUser);
        Assert.Equal(Goal.MuscleGain, memberUser.Goal);
        Assert.Equal(2500, memberUser.EstimatedCaloriesIntake);
    }

    [Fact]
    public void GetUsers_ShouldHaveJaneUser()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        
        // اطمینان از وجود janesmith در لیست persons
        var janePerson = persons.FirstOrDefault(p => p.Username == "janesmith");
        Assert.NotNull(janePerson); // اگر این خطا داد، مشکل در GetPersons است

        // Act
        var users = HardCodedData.GetUsers(persons);
        var janeUser = users.FirstOrDefault(u => u.PersonId == janePerson.Id);

        // Assert
        Assert.NotNull(janeUser);
        Assert.Equal(Goal.FatLoss, janeUser.Goal);
        Assert.Equal(1800, janeUser.EstimatedCaloriesIntake);
    }

    // ========== Tests for GetCoaches ==========

    [Fact]
    public void GetCoaches_ShouldReturnCorrectCount()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();

        // Act
        var coaches = HardCodedData.GetCoaches(persons);

        // Assert
        Assert.NotNull(coaches);
        Assert.Single(coaches);
    }

    [Fact]
    public void GetCoaches_ShouldHaveCoach()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var coachPerson = persons.First(p => p.Username == "coach");

        // Act
        var coaches = HardCodedData.GetCoaches(persons);
        var coach = coaches.First();

        // Assert
        Assert.Equal(coachPerson.Id, coach.PersonId);
        Assert.Equal("Strength & Conditioning", coach.Specialization);
        Assert.Equal(10, coach.YearsOfExperience);
    }

    // ========== Tests for GetWorkoutPlans ==========

    [Fact]
    public void GetWorkoutPlans_WithUsers_ShouldReturnPlan()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);

        // Act
        var plans = HardCodedData.GetWorkoutPlans(users);

        // Assert
        Assert.NotNull(plans);
        Assert.Single(plans);
        var plan = plans.First();
        Assert.Equal(users.First().Id, plan.UserId);
        Assert.Equal(1, plan.Phase);
        Assert.Equal(3, plan.SessionsPerWeek);
        Assert.True(plan.IsActive);
    }

    [Fact]
    public void GetWorkoutPlans_WithEmptyUsers_ShouldReturnEmpty()
    {
        // Act
        var plans = HardCodedData.GetWorkoutPlans(new List<User>());

        // Assert
        Assert.NotNull(plans);
        Assert.Empty(plans);
    }

    // ========== Tests for GetWorkoutDays ==========

    [Fact]
    public void GetWorkoutDays_WithPlans_ShouldReturnThreeDays()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);

        // Act
        var days = HardCodedData.GetWorkoutDays(plans);

        // Assert
        Assert.NotNull(days);
        Assert.Equal(3, days.Count);
    }

    [Fact]
    public void GetWorkoutDays_ShouldHaveCorrectWeekdays()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);

        // Act
        var days = HardCodedData.GetWorkoutDays(plans);
        var weekdays = days.Select(d => d.DayOfWeek).ToList();

        // Assert
        Assert.Contains(DayOfWeek.Monday, weekdays);
        Assert.Contains(DayOfWeek.Wednesday, weekdays);
        Assert.Contains(DayOfWeek.Friday, weekdays);
    }

    [Fact]
    public void GetWorkoutDays_ShouldHaveCorrectTargetMuscles()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);

        // Act
        var days = HardCodedData.GetWorkoutDays(plans);
        var monday = days.First(d => d.DayOfWeek == DayOfWeek.Monday);
        var wednesday = days.First(d => d.DayOfWeek == DayOfWeek.Wednesday);
        var friday = days.First(d => d.DayOfWeek == DayOfWeek.Friday);

        // Assert
        Assert.Equal(MuscleGroup.Chest, monday.TargetMuscles);
        Assert.Equal(MuscleGroup.Back, wednesday.TargetMuscles);
        Assert.Equal(MuscleGroup.Legs, friday.TargetMuscles);
    }

    [Fact]
    public void GetWorkoutDays_WithNoPlans_ShouldReturnEmpty()
    {
        // Act
        var days = HardCodedData.GetWorkoutDays(new List<WorkoutPlan>());

        // Assert
        Assert.NotNull(days);
        Assert.Empty(days);
    }

    // ========== Tests for GetWorkoutDayExercises ==========

    [Fact]
    public void GetWorkoutDayExercises_ShouldReturnExercises()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);
        var days = HardCodedData.GetWorkoutDays(plans);
        var exercises = HardCodedData.GetExercises();

        // Act
        var wdes = HardCodedData.GetWorkoutDayExercises(days, exercises);

        // Assert
        Assert.NotNull(wdes);
        Assert.True(wdes.Count >= 4);
    }

    [Fact]
    public void GetWorkoutDayExercises_MondayShouldHaveBenchPress()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);
        var days = HardCodedData.GetWorkoutDays(plans);
        var exercises = HardCodedData.GetExercises();
        var monday = days.First(d => d.DayOfWeek == DayOfWeek.Monday);

        // Act
        var wdes = HardCodedData.GetWorkoutDayExercises(days, exercises);
        var mondayExercises = wdes.Where(wde => wde.WorkoutDayId == monday.Id).ToList();

        // Assert
        Assert.Single(mondayExercises);
        var benchExercise = exercises.First(e => e.Name == "Bench Press");
        Assert.Equal(benchExercise.Id, mondayExercises.First().ExerciseId);
        Assert.Equal(3, mondayExercises.First().Sets);
        Assert.Equal("10,10,8", mondayExercises.First().Reps);
    }

    [Fact]
    public void GetWorkoutDayExercises_WednesdayShouldHaveDeadliftAndPullUp()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);
        var days = HardCodedData.GetWorkoutDays(plans);
        var exercises = HardCodedData.GetExercises();
        var wednesday = days.First(d => d.DayOfWeek == DayOfWeek.Wednesday);

        // Act
        var wdes = HardCodedData.GetWorkoutDayExercises(days, exercises);
        var wednesdayExercises = wdes.Where(wde => wde.WorkoutDayId == wednesday.Id).ToList();

        // Assert
        Assert.Equal(2, wednesdayExercises.Count);
    }

    // ========== Tests for GetProgressLogs ==========

    [Fact]
    public void GetProgressLogs_ShouldReturnThreeLogs()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);

        // Act
        var logs = HardCodedData.GetProgressLogs(users, plans);

        // Assert
        Assert.NotNull(logs);
        Assert.Equal(3, logs.Count);
    }

    [Fact]
    public void GetProgressLogs_ShouldShowWeightDecrease()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);

        // Act
        var logs = HardCodedData.GetProgressLogs(users, plans);
        var firstWeight = logs.First().Weight;
        var lastWeight = logs.Last().Weight;

        // Assert
        Assert.True(lastWeight < firstWeight, "Weight should decrease over time");
    }

    [Fact]
    public void GetProgressLogs_WithNoUsers_ShouldReturnEmpty()
    {
        // Act
        var logs = HardCodedData.GetProgressLogs(new List<User>(), new List<WorkoutPlan>());

        // Assert
        Assert.NotNull(logs);
        Assert.Empty(logs);
    }

    // ========== Tests for GetWorkoutSessions ==========

    [Fact]
    public void GetWorkoutSessions_ShouldReturnThreeSessions()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);
        var days = HardCodedData.GetWorkoutDays(plans);

        // Act
        var sessions = HardCodedData.GetWorkoutSessions(days);

        // Assert
        Assert.NotNull(sessions);
        Assert.Equal(3, sessions.Count);
    }

    [Fact]
    public void GetWorkoutSessions_ShouldHaveCorrectDuration()
    {
        // Arrange
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);
        var days = HardCodedData.GetWorkoutDays(plans);

        // Act
        var sessions = HardCodedData.GetWorkoutSessions(days);

        // Assert
        foreach (var session in sessions)
        {
            Assert.Equal(60, session.ActualDurationMinutes);
            Assert.Equal("Great workout!", session.Feeling);
        }
    }

    [Fact]
    public void GetWorkoutSessions_WithNoDays_ShouldReturnEmpty()
    {
        // Act
        var sessions = HardCodedData.GetWorkoutSessions(new List<WorkoutDay>());

        // Assert
        Assert.NotNull(sessions);
        Assert.Empty(sessions);
    }

    // ========== Integration Tests ==========

    [Fact]
    public void FullDataGeneration_ShouldCreateValidRelationships()
    {
        // Arrange & Act
        var persons = HardCodedData.GetPersons();
        var users = HardCodedData.GetUsers(persons);
        var coaches = HardCodedData.GetCoaches(persons);
        var plans = HardCodedData.GetWorkoutPlans(users);
        var days = HardCodedData.GetWorkoutDays(plans);
        var exercises = HardCodedData.GetExercises();
        var wdes = HardCodedData.GetWorkoutDayExercises(days, exercises);
        var logs = HardCodedData.GetProgressLogs(users, plans);
        var sessions = HardCodedData.GetWorkoutSessions(days);

        // Assert
        Assert.Equal(3, persons.Count);
        Assert.Equal(2, users.Count);
        Assert.Single(coaches);
        Assert.Single(plans);
        Assert.Equal(3, days.Count);
        Assert.Equal(10, exercises.Count);
        Assert.True(wdes.Count >= 4);
        Assert.Equal(3, logs.Count);
        Assert.Equal(3, sessions.Count);
    }
}