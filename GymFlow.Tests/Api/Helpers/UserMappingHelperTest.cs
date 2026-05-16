using GymFlow.Api.Helpers;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Models.Entities;
using GymFlow.Models.Enums;

namespace GymFlow.Tests.Api.Helpers;

public class UserMappingHelperTest
{
    #region Helper Methods

    private User CreateTestUser(
        int id = 1,
        string firstName = "John",
        string lastName = "Doe",
        string? email = "john@example.com",
        string? phone = "123456789",
        Gender gender = Gender.Male,
        int age = 30,
        float weight = 80f,
        float height = 180f,
        BodyType? bodyType = BodyType.Fit,
        Goal goal = Goal.Fitness,
        int? estimatedCalories = 2500,
        bool isCompetitive = false,
        DateTime? createdAt = null)
    {
        var person = new Person
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Gender = gender,
            Age = age,
            Weight = weight,
            Height = height,
            BodyType = bodyType,
            Username = email ?? "username",
            Password = "pass",
            CreatedAt = createdAt ?? DateTime.UtcNow
        };

        return new User
        {
            Id = id,
            PersonId = person.Id,
            Person = person,
            Goal = goal,
            EstimatedCaloriesIntake = estimatedCalories,
            IsCompetitive = isCompetitive,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            WorkoutPlans = new List<WorkoutPlan>(),
            ProgressLogs = new List<ProgressLog>()
        };
    }

    #endregion

    #region ToUserResponse

    [Fact]
    public void ToUserResponse_WithNullUser_ReturnsEmptyResponse()
    {
        // Act
        var result = UserMappingHelper.ToUserResponse(null!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal(string.Empty, result.FirstName);   // تغییر: از Null به Empty
        Assert.Equal(string.Empty, result.LastName);    // تغییر: از Null به Empty
        Assert.Null(result.Email);                      // ایمیل nullable است، بنابراین null باقی می‌ماند
    }

    [Fact]
    public void ToUserResponse_WithUserHavingNullPerson_ReturnsEmptyResponse()
    {
        // Arrange
        var user = new User { Id = 1, Person = null! };

        // Act
        var result = UserMappingHelper.ToUserResponse(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal(string.Empty, result.FirstName);   // تغییر: از Null به Empty
        Assert.Equal(string.Empty, result.LastName);    // تغییر: از Null به Empty
        Assert.Null(result.Email);                      // ایمیل nullable است
    }

    [Fact]
    public void ToUserResponse_WithValidUser_ReturnsPopulatedResponse()
    {
        // Arrange
        var user = CreateTestUser(
            id: 5,
            firstName: "Jane",
            lastName: "Smith",
            email: "jane@test.com",
            phone: "987654321",
            gender: Gender.Female,
            age: 28,
            weight: 65f,
            height: 165f,
            bodyType: BodyType.LeanMuscular,
            goal: Goal.FatLoss,
            estimatedCalories: 1800,
            isCompetitive: true
        );

        // Act
        var result = UserMappingHelper.ToUserResponse(user);

        // Assert
        Assert.Equal(5, result.Id);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Smith", result.LastName);
        Assert.Equal("jane@test.com", result.Email);
        Assert.Equal("987654321", result.Phone);
        Assert.Equal(Gender.Female, result.Gender);
        Assert.Equal(28, result.Age);
        Assert.Equal(65f, result.Weight);
        Assert.Equal(165f, result.Height);
        Assert.Equal(BodyType.LeanMuscular, result.BodyType);
        Assert.Equal(Goal.FatLoss, result.Goal);
        Assert.Equal(1800, result.EstimatedCaloriesIntake);
        Assert.True(result.IsCompetitive);
        Assert.Equal(0, result.WorkoutPlansCount);
        Assert.Equal(0, result.ProgressLogsCount);
        Assert.Equal(0, result.TotalWorkoutSessions);
        Assert.Equal(user.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public void ToUserResponse_WithNullOptionalFields_HandlesGracefully()
    {
        // Arrange
        var user = CreateTestUser(email: null, phone: null, bodyType: null, estimatedCalories: null);

        // Act
        var result = UserMappingHelper.ToUserResponse(user);

        // Assert
        Assert.Null(result.Email);
        Assert.Null(result.Phone);
        Assert.Null(result.BodyType);
        Assert.Null(result.EstimatedCaloriesIntake);
    }

    [Fact]
    public void ToUserResponse_CountsWorkoutPlansAndProgressLogs()
    {
        // Arrange
        var user = CreateTestUser();
        user.WorkoutPlans = new List<WorkoutPlan> { new WorkoutPlan(), new WorkoutPlan() };
        user.ProgressLogs = new List<ProgressLog> { new ProgressLog(), new ProgressLog(), new ProgressLog() };

        // Act
        var result = UserMappingHelper.ToUserResponse(user);

        // Assert
        Assert.Equal(2, result.WorkoutPlansCount);
        Assert.Equal(3, result.ProgressLogsCount);
    }

    #endregion

    #region ToUser

    [Fact]
    public void ToUser_WithValidRequest_CreatesUserWithPerson()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "Alice",
            LastName = "Wonder",
            Email = "alice@test.com",
            Phone = "111222333",
            Gender = Gender.Female,
            Age = 25,
            Weight = 60f,
            Height = 165f,
            BodyType = BodyType.Fit,
            Goal = Goal.Fitness,
            EstimatedCaloriesIntake = 2000,
            IsCompetitive = false
        };

        // Act
        var result = UserMappingHelper.ToUser(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Person);
        Assert.Equal(request.FirstName, result.Person.FirstName);
        Assert.Equal(request.LastName, result.Person.LastName);
        Assert.Equal(request.Email, result.Person.Email);
        Assert.Equal(request.Phone, result.Person.Phone);
        Assert.Equal(request.Gender, result.Person.Gender);
        Assert.Equal(request.Age, result.Person.Age);
        Assert.Equal(request.Weight, result.Person.Weight);
        Assert.Equal(request.Height, result.Person.Height);
        Assert.Equal(request.BodyType, result.Person.BodyType);
        Assert.Equal(request.Email ?? string.Empty, result.Person.Username);
        Assert.Equal("temp123", result.Person.Password);
        Assert.Equal(request.Goal, result.Goal);
        Assert.Equal(request.EstimatedCaloriesIntake, result.EstimatedCaloriesIntake);
        Assert.Equal(request.IsCompetitive, result.IsCompetitive);
    }

    [Fact]
    public void ToUser_WithNullEmail_SetsUsernameToEmptyString()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = null,
            Gender = Gender.Male,
            Age = 20,
            Goal = Goal.Fitness
        };

        // Act
        var result = UserMappingHelper.ToUser(request);

        // Assert
        Assert.Equal(string.Empty, result.Person.Username);
    }

    [Fact]
    public void ToUser_WithNullOptionalFields_CreatesUserWithNulls()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FirstName = "Test",
            LastName = "User",
            Gender = Gender.Male,
            Age = 20,
            Goal = Goal.Fitness,
            Email = null,
            Phone = null,
            Weight = null,
            Height = null,
            BodyType = null,
            EstimatedCaloriesIntake = null
        };

        // Act
        var result = UserMappingHelper.ToUser(request);

        // Assert
        Assert.Null(result.Person.Email);
        Assert.Null(result.Person.Phone);
        Assert.Null(result.Person.Weight);
        Assert.Null(result.Person.Height);
        Assert.Null(result.Person.BodyType);
        Assert.Null(result.EstimatedCaloriesIntake);
    }

    #endregion

    #region UpdateUserFromRequest

    [Fact]
    public void UpdateUserFromRequest_WithNullUser_DoesNothing()
    {
        // Arrange
        User? user = null;
        var request = new UpdateUserRequest { FirstName = "New" };

        // Act
        UserMappingHelper.UpdateUserFromRequest(user!, request);

        // Assert - No exception, nothing changes
        Assert.Null(user);
    }

    [Fact]
    public void UpdateUserFromRequest_WithUserHavingNullPerson_DoesNothing()
    {
        // Arrange
        var user = new User { Id = 1, Person = null! };
        var request = new UpdateUserRequest { FirstName = "New" };

        // Act
        UserMappingHelper.UpdateUserFromRequest(user, request);

        // Assert - No change
        Assert.Null(user.Person);
    }

    [Fact]
    public void UpdateUserFromRequest_WithAllFieldsProvided_UpdatesAllProperties()
    {
        // Arrange
        var user = CreateTestUser();
        var request = new UpdateUserRequest
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            Email = "updated@test.com",
            Phone = "999888777",
            Gender = Gender.Female,
            Age = 35,
            Weight = 90f,
            Height = 185f,
            BodyType = BodyType.Overweight,
            Goal = Goal.MuscleGain,
            EstimatedCaloriesIntake = 3000,
            IsCompetitive = true
        };

        // Act
        UserMappingHelper.UpdateUserFromRequest(user, request);

        // Assert
        Assert.Equal(request.FirstName, user.Person.FirstName);
        Assert.Equal(request.LastName, user.Person.LastName);
        Assert.Equal(request.Email, user.Person.Email);
        Assert.Equal(request.Phone, user.Person.Phone);
        Assert.Equal(request.Gender, user.Person.Gender);
        Assert.Equal(request.Age, user.Person.Age);
        Assert.Equal(request.Weight, user.Person.Weight);
        Assert.Equal(request.Height, user.Person.Height);
        Assert.Equal(request.BodyType, user.Person.BodyType);
        Assert.Equal(request.Goal, user.Goal);
        Assert.Equal(request.EstimatedCaloriesIntake, user.EstimatedCaloriesIntake);
        Assert.Equal(request.IsCompetitive, user.IsCompetitive);
    }

    [Fact]
    public void UpdateUserFromRequest_WithPartialFields_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var user = CreateTestUser(
            firstName: "Original",
            lastName: "Original",
            email: "orig@test.com",
            phone: "111",
            gender: Gender.Male,
            age: 20,
            weight: 70f,
            height: 170f,
            bodyType: BodyType.Fit,
            goal: Goal.Fitness,
            estimatedCalories: 2000,
            isCompetitive: false
        );

        var request = new UpdateUserRequest
        {
            FirstName = "NewFirst",
            Weight = 85f,
            Goal = Goal.FatLoss
        };

        // Act
        UserMappingHelper.UpdateUserFromRequest(user, request);

        // Assert
        Assert.Equal("NewFirst", user.Person.FirstName);
        Assert.Equal("Original", user.Person.LastName);  // unchanged
        Assert.Equal("orig@test.com", user.Person.Email); // unchanged
        Assert.Equal("111", user.Person.Phone); // unchanged
        Assert.Equal(Gender.Male, user.Person.Gender); // unchanged
        Assert.Equal(20, user.Person.Age); // unchanged
        Assert.Equal(85f, user.Person.Weight); // updated
        Assert.Equal(170f, user.Person.Height); // unchanged
        Assert.Equal(BodyType.Fit, user.Person.BodyType); // unchanged
        Assert.Equal(Goal.FatLoss, user.Goal); // updated
        Assert.Equal(2000, user.EstimatedCaloriesIntake); // unchanged
        Assert.False(user.IsCompetitive); // unchanged
    }

    [Fact]
    public void UpdateUserFromRequest_WithNullValues_DoesNotOverrideExisting()
    {
        // Arrange
        var user = CreateTestUser(firstName: "Original", email: "orig@test.com");
        var request = new UpdateUserRequest
        {
            FirstName = null,
            Email = null,
            Age = null
        };

        // Act
        UserMappingHelper.UpdateUserFromRequest(user, request);

        // Assert
        Assert.Equal("Original", user.Person.FirstName);
        Assert.Equal("orig@test.com", user.Person.Email);
        Assert.Equal(30, user.Person.Age); // unchanged
    }

    [Fact]
    public void UpdateUserFromRequest_WithEmptyStrings_UpdatesToEmpty()
    {
        // Arrange
        var user = CreateTestUser(firstName: "Original", lastName: "Original", email: "orig@test.com", phone: "123");
        var request = new UpdateUserRequest
        {
            FirstName = "",
            LastName = "",
            Email = "",
            Phone = ""
        };

        // Act
        UserMappingHelper.UpdateUserFromRequest(user, request);

        // Assert
        Assert.Equal("", user.Person.FirstName);
        Assert.Equal("", user.Person.LastName);
        Assert.Equal("", user.Person.Email);
        Assert.Equal("", user.Person.Phone);
    }

    #endregion
}