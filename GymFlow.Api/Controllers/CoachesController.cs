using Microsoft.AspNetCore.Mvc;
using GymFlow.Api.Controllers.Base;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.DTOs.Responses;

namespace GymFlow.Api.Controllers;

[Tags("Coaches")]
public class CoachesController : ApiControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ICoachRepository _coachRepository;
    private readonly IPersonRepository _personRepository; // اضافه شد
    private readonly IProgressLogRepository _progressLogRepository;
    private readonly IWorkoutSessionRepository _workoutSessionRepository;

    public CoachesController
    (
        IUserRepository userRepository,
        ICoachRepository coachRepository,
        IPersonRepository personRepository,
        IProgressLogRepository progressLogRepository,
        IWorkoutSessionRepository workoutSessionRepository)
    {
        _userRepository = userRepository;
        _coachRepository = coachRepository;
        _personRepository = personRepository;
        _progressLogRepository = progressLogRepository;
        _workoutSessionRepository = workoutSessionRepository;
    }

    [HttpGet("{userId:int}/clients")]
    public async Task<IActionResult> GetClientsAsync(int userId)
    {
        // بررسی دسترسی: فقط خود مربی می‌تواند مشتریانش را ببیند
        var currentUserId = HttpContext.Items["UserId"] as int?;
        if (currentUserId == null || currentUserId != userId)
            return Unauthorized();

        var user = await _userRepository.GetUserWithPersonAsync(userId);
        if (user == null || user.Person == null)
            return NotFoundResponse("User", userId);

        var coach = await _coachRepository.GetByPersonIdAsync(user.Person.Id);
        if (coach == null)
            return NotFoundResponse("Coach for user", userId);

        // اصلاح: استفاده از متدی که Person را Include کند
        var clients = await _userRepository.GetUsersByCoachIdWithPersonAsync(coach.Id);
        clients = clients.Where(u => u.CoachId == coach.Id).ToList();

        var result = new List<object>();
        foreach (var client in clients)
        {
            var latestLog = await _progressLogRepository.GetLatestProgressLogAsync(client.Id);
            var sessionsCount = await _workoutSessionRepository.GetSessionCountByUserAsync(client.Id);
            
            result.Add(new
            {
                client.Id,
                FullName = client.Person == null ? "Unknown" : $"{client.Person.FirstName} {client.Person.LastName}",
                Goal = client.Goal.ToString(),
                CurrentWeight = latestLog?.Weight ?? client.Person?.Weight ?? 0,
                CompletedSessions = sessionsCount
            });
        }

        return Success(result);
    }

    // ========== NEW: دریافت پروفایل مربی جاری ==========
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfileAsync()
    {
        // گرفتن UserId از HttpContext (تنظیم شده در BasicAuthMiddleware)
        var userId = HttpContext.Items["UserId"] as int?;
        if (userId == null)
            return Unauthorized();

        var coach = await _coachRepository.GetByUserIdAsync(userId.Value);
        if (coach == null || coach.Person == null)
            return NotFoundResponse("Coach profile");

        var response = new CoachProfileResponse
        {
            FirstName = coach.Person.FirstName,
            LastName = coach.Person.LastName,
            Email = coach.Person.Email,
            Phone = coach.Person.Phone,
            Specialization = coach.Specialization,
            YearsOfExperience = coach.YearsOfExperience,
            Username = coach.Person.Username,
            CreatedAt = coach.CreatedAt
        };

        return Success(response);
    }

    // ========== NEW: به‌روزرسانی پروفایل مربی جاری ==========
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfileAsync([FromBody] UpdateCoachProfileRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationErrorResponse();

        var userId = HttpContext.Items["UserId"] as int?;
        if (userId == null)
            return Unauthorized();

        var coach = await _coachRepository.GetByUserIdAsync(userId.Value);
        if (coach == null || coach.Person == null)
            return NotFoundResponse("Coach profile");

        // به‌روزرسانی اطلاعات Person
        coach.Person.FirstName = request.FirstName;
        coach.Person.LastName = request.LastName;
        coach.Person.Email = request.Email;
        coach.Person.Phone = request.Phone;

        // به‌روزرسانی اطلاعات Coach
        coach.Specialization = request.Specialization;
        coach.YearsOfExperience = request.YearsOfExperience;

        // ذخیره تغییرات
        await _personRepository.UpdateAsync(coach.Person);
        await _coachRepository.UpdateAsync(coach);

        return Success<object>(null, "Profile updated successfully");
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAllCoachesAsync()
    {
        var coaches = await _coachRepository.GetAllCoachesWithPersonAsync();
        
        var result = coaches.Select(c => new
        {
            c.Id,
            FullName = c.Person == null ? "Unknown" : $"{c.Person.FirstName} {c.Person.LastName}",
            Specialization = c.Specialization
        });
        
        return Success(result);
    }
}