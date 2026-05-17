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

    public CoachesController
    (
        IUserRepository userRepository,
        ICoachRepository coachRepository,
        IPersonRepository personRepository)
    {
        _userRepository = userRepository;
        _coachRepository = coachRepository;
        _personRepository = personRepository;
    }

    [HttpGet("{userId:int}/clients")]
    public async Task<IActionResult> GetClientsAsync(int userId)
    {
        var user = await _userRepository.GetUserWithPersonAsync(userId);
        if (user == null)
            return NotFoundResponse("User", userId);

        var person = user.Person;
        if (person == null)
            return NotFoundResponse("Person for user", userId);

        var coach = await _coachRepository.GetByPersonIdAsync(person.Id);
        if (coach == null)
            return NotFoundResponse("Coach for user", userId);

        var clients = await _userRepository.FindAsync(u => u.CoachId == coach.Id);
        var result = clients.Select(c => new
        {
            c.Id,
            FullName = c.Person == null ? "Unknown" : $"{c.Person.FirstName} {c.Person.LastName}",
            c.Goal,
            CurrentWeight = c.Person?.Weight
        });

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
}