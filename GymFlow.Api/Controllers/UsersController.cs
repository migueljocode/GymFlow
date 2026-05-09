using Microsoft.AspNetCore.Mvc;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Models.Entities;
using GymFlow.Api.Controllers.Base;

namespace GymFlow.Api.Controllers;

/// <summary>
/// Controller for managing users
/// </summary>
[Tags("Users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly IProgressLogRepository _progressLogRepository;

    public UsersController(
        IUserRepository userRepository,
        IWorkoutPlanRepository workoutPlanRepository,
        IWorkoutSessionRepository workoutSessionRepository,
        IProgressLogRepository progressLogRepository)
    {
        _userRepository = userRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _workoutSessionRepository = workoutSessionRepository;
        _progressLogRepository = progressLogRepository;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _userRepository.GetAllAsync();
        var userList = users.ToList();
        var totalCount = userList.Count;
        var pagedUsers = userList.Skip((page - 1) * pageSize).Take(pageSize);
        
        var responses = new List<UserResponse>();
        foreach (var user in pagedUsers)
        {
            responses.Add(await MapToResponseAsync(user));
        }
        
        return Success<IEnumerable<UserResponse>>(responses);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetUserWithCompleteHistoryAsync(id);
        if (user is null)
            return NotFoundResponse("User", id);
        
        var response = await MapToResponseAsync(user);
        return Success<UserResponse>(response);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user is null)
            return NotFoundResponse("User", email);
        
        var response = await MapToResponseAsync(user);
        return Success<UserResponse>(response);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        // Check if email already exists
        if (!string.IsNullOrEmpty(request.Email))
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser is not null)
                return Error("A user with this email already exists", 409);
        }
        
        var user = new User
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
            Goal = request.Goal,
            EstimatedCaloriesIntake = request.EstimatedCaloriesIntake,
            IsCompetitive = request.IsCompetitive
        };
        
        var created = await _userRepository.AddAsync(user);
        var response = await MapToResponseAsync(created);
        
        return CreatedResponse<UserResponse>(response, "User created successfully");
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return NotFoundResponse("User", id);
        
        // Update only provided fields
        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.Email is not null) user.Email = request.Email;
        if (request.Phone is not null) user.Phone = request.Phone;
        if (request.Gender.HasValue) user.Gender = request.Gender.Value;
        if (request.Age.HasValue) user.Age = request.Age.Value;
        if (request.Weight.HasValue) user.Weight = request.Weight.Value;
        if (request.Height.HasValue) user.Height = request.Height.Value;
        if (request.BodyType.HasValue) user.BodyType = request.BodyType.Value;
        if (request.Goal.HasValue) user.Goal = request.Goal.Value;
        if (request.EstimatedCaloriesIntake.HasValue) user.EstimatedCaloriesIntake = request.EstimatedCaloriesIntake;
        if (request.IsCompetitive.HasValue) user.IsCompetitive = request.IsCompetitive.Value;
        
        var updated = await _userRepository.UpdateAsync(user);
        var response = await MapToResponseAsync(updated);
        
        return Success<UserResponse>(response, "User updated successfully");
    }

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return NotFoundResponse("User", id);
        
        await _userRepository.SoftDeleteAsync(id);
        return Success<string?>(null, "User deleted successfully");
    }

    /// <summary>
    /// Get user's progress summary
    /// </summary>
    [HttpGet("{id:int}/summary")]
    public async Task<IActionResult> GetUserSummaryAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return NotFoundResponse("User", id);
        
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(id);
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(id);
        
        var latestLog = logs.FirstOrDefault();
        var lastWeekLog = logs.Skip(1).FirstOrDefault();
        var lastMonthLog = logs.Skip(3).FirstOrDefault();
        
        var summary = new
        {
            userId = user.Id,
            userName = $"{user.FirstName} {user.LastName}",
            currentWeight = latestLog?.Weight ?? user.Weight,
            startingWeight = logs.LastOrDefault()?.Weight,
            weightChange = latestLog != null && logs.LastOrDefault() != null 
                ? latestLog.Weight - logs.Last().Weight 
                : (float?)null,
            weightChangeLastWeek = lastWeekLog != null && latestLog != null 
                ? latestLog.Weight - lastWeekLog.Weight 
                : (float?)null,
            weightChangeLastMonth = lastMonthLog != null && latestLog != null 
                ? latestLog.Weight - lastMonthLog.Weight 
                : (float?)null,
            totalWorkoutSessions = sessions.Count(),
            totalProgressLogs = logs.Count(),
            memberSince = user.CreatedAt
        };
        
        return Success<object>(summary);
    }

    private async Task<UserResponse> MapToResponseAsync(User user)
    {
        var workoutPlans = await _workoutPlanRepository.GetUserWorkoutPlansAsync(user.Id);
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(user.Id);
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(user.Id);
        
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Gender = user.Gender,
            Age = user.Age,
            Weight = user.Weight,
            Height = user.Height,
            BodyType = user.BodyType,
            Goal = user.Goal,
            EstimatedCaloriesIntake = user.EstimatedCaloriesIntake,
            IsCompetitive = user.IsCompetitive,
            WorkoutPlansCount = workoutPlans.Count(),
            ProgressLogsCount = logs.Count(),
            TotalWorkoutSessions = sessions.Count(),
            CreatedAt = user.CreatedAt
        };
    }
}