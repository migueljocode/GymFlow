using Microsoft.AspNetCore.Mvc;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.DTOs.Responses;
using GymFlow.Api.Controllers.Base;
using GymFlow.Api.Helpers;

namespace GymFlow.Api.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var users = await _userRepository.GetAllUsersWithPersonAsync();
        var responses = users.Select(UserMappingHelper.ToUserResponse);
        return Success<IEnumerable<UserResponse>>(responses);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetUserWithCompleteHistoryAsync(id);
        if (user is null)
            return NotFoundResponse("User", id);
        
        var response = UserMappingHelper.ToUserResponse(user);
        
        // اضافه کردن CoachId به پاسخ
        var result = new
        {
            response.Id,
            response.FirstName,
            response.LastName,
            response.FullName,
            response.Email,
            response.Phone,
            response.Gender,
            response.Age,
            response.Weight,
            response.Height,
            response.BodyType,
            response.Goal,
            response.EstimatedCaloriesIntake,
            response.IsCompetitive,
            response.WorkoutPlansCount,
            response.ProgressLogsCount,
            response.TotalWorkoutSessions,
            response.CreatedAt,
            response.Username,
            CoachId = user.CoachId  // اضافه شد
        };
        
        return Success(result);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user is null)
            return NotFoundResponse("User", email);
        
        var response = UserMappingHelper.ToUserResponse(user);
        return Success<UserResponse>(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        // چک کردن تکراری نبودن ایمیل
        if (!string.IsNullOrEmpty(request.Email))
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser is not null)
                return Error("A user with this email already exists", 409);
        }
        
        var user = UserMappingHelper.ToUser(request);
        var created = await _userRepository.AddAsync(user);
        var response = UserMappingHelper.ToUserResponse(created);
        
        return CreatedResponse<UserResponse>(response, "User created successfully");
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();
        
        var user = await _userRepository.GetUserWithPersonAsync(id);
        if (user is null)
            return NotFoundResponse("User", id);
        
        // لاگ برای دیباگ
        Console.WriteLine($"[DEBUG] UpdateAsync - Received CoachId: {request.CoachId}");
        
        UserMappingHelper.UpdateUserFromRequest(user, request);
        
        // اگر CoachId در ریکوئست وجود دارد (حتی اگر null باشد)
        if (request.CoachId.HasValue)
        {
            user.CoachId = request.CoachId.Value;
            Console.WriteLine($"[DEBUG] Setting CoachId to {request.CoachId.Value}");
        }
        else
        {
            // اگر CoachId در ریکوئست null است، یعنی کاربر می‌خواهد مربی را حذف کند
            user.CoachId = null;
            Console.WriteLine($"[DEBUG] Setting CoachId to null");
        }
        
        var updated = await _userRepository.UpdateAsync(user);
        var response = UserMappingHelper.ToUserResponse(updated);
        
        return Success<UserResponse>(response, "User updated successfully");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
            return NotFoundResponse("User", id);
        
        await _userRepository.SoftDeleteAsync(id);
        return Success<string?>(null, "User deleted successfully");
    }

    [HttpGet("{id:int}/summary")]
    public async Task<IActionResult> GetUserSummaryAsync(int id)
    {
        var user = await _userRepository.GetUserWithPersonAsync(id);
        if (user is null)
            return NotFoundResponse("User", id);
        
        var logs = await _progressLogRepository.GetUserProgressHistoryAsync(id);
        var sessions = await _workoutSessionRepository.GetSessionsByUserAsync(id);
        
        var latestLog = logs.FirstOrDefault();
        var firstLog = logs.LastOrDefault();
        
        var summary = new
        {
            userId = user.Id,
            userName = user.Person?.FullName ?? "Unknown",
            currentWeight = latestLog?.Weight ?? user.Person?.Weight,
            startingWeight = firstLog?.Weight,
            weightChange = latestLog != null && firstLog != null 
                ? latestLog.Weight - firstLog.Weight 
                : (float?)null,
            totalWorkoutSessions = sessions.Count(),
            totalProgressLogs = logs.Count(),
            memberSince = user.CreatedAt
        };
        
        return Success<object>(summary);
    }
}