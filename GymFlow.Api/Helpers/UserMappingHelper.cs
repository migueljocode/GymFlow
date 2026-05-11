using GymFlow.Models.Entities;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Models.DTOs.Requests;

namespace GymFlow.Api.Helpers;

public static class UserMappingHelper
{
    /// <summary>
    /// تبدیل User به UserResponse
    /// </summary>
    public static UserResponse ToUserResponse(User user)
    {
        if (user?.Person == null)
            return new UserResponse();
        
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.Person.FirstName,
            LastName = user.Person.LastName,
            // FullName حذف شد چون فقط خواندنی است
            Email = user.Person.Email,
            Phone = user.Person.Phone,
            Gender = user.Person.Gender,
            Age = user.Person.Age,
            Weight = user.Person.Weight,
            Height = user.Person.Height,
            BodyType = user.Person.BodyType,
            Goal = user.Goal,
            EstimatedCaloriesIntake = user.EstimatedCaloriesIntake,
            IsCompetitive = user.IsCompetitive,
            WorkoutPlansCount = user.WorkoutPlans?.Count ?? 0,
            ProgressLogsCount = user.ProgressLogs?.Count ?? 0,
            TotalWorkoutSessions = 0,
            CreatedAt = user.CreatedAt
        };
    }
    
    /// <summary>
    /// تبدیل CreateUserRequest به User (همراه با Person)
    /// </summary>
    public static User ToUser(CreateUserRequest request)
    {
        var person = new Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Gender = request.Gender,
            Age = request.Age,
            Weight = request.Weight,
            Height = request.Height,
            BodyType = request.BodyType,
            Username = request.Email ?? string.Empty, // اگر null بود، رشته خالی بگذار
            Password = "temp123"
        };
        
        return new User
        {
            Person = person,
            Goal = request.Goal,
            EstimatedCaloriesIntake = request.EstimatedCaloriesIntake,
            IsCompetitive = request.IsCompetitive
        };
    }
    
    /// <summary>
    /// به‌روزرسانی User و Person از UpdateUserRequest
    /// </summary>
    public static void UpdateUserFromRequest(User user, UpdateUserRequest request)
    {
        if (user?.Person == null) return;
        
        if (request.FirstName is not null) user.Person.FirstName = request.FirstName;
        if (request.LastName is not null) user.Person.LastName = request.LastName;
        if (request.Email is not null) user.Person.Email = request.Email;
        if (request.Phone is not null) user.Person.Phone = request.Phone;
        if (request.Gender.HasValue) user.Person.Gender = request.Gender.Value;
        if (request.Age.HasValue) user.Person.Age = request.Age.Value;
        if (request.Weight.HasValue) user.Person.Weight = request.Weight.Value;
        if (request.Height.HasValue) user.Person.Height = request.Height.Value;
        if (request.BodyType.HasValue) user.Person.BodyType = request.BodyType.Value;
        if (request.Goal.HasValue) user.Goal = request.Goal.Value;
        if (request.EstimatedCaloriesIntake.HasValue) user.EstimatedCaloriesIntake = request.EstimatedCaloriesIntake;
        if (request.IsCompetitive.HasValue) user.IsCompetitive = request.IsCompetitive.Value;
    }
}