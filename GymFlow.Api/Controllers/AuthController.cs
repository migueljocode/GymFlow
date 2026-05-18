using Microsoft.AspNetCore.Mvc;
using GymFlow.Api.Controllers.Base;
using GymFlow.Services.Interfaces;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.Entities;
using GymFlow.Dal.Repositories.Interfaces;
using GymFlow.Models.Enums;

namespace GymFlow.Api.Controllers;

[Tags("Authentication")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICoachRepository _coachRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUserRepository _userRepository;

    public AuthController
    (
        IAuthService authService,
        ICoachRepository coachRepository,
        IPersonRepository personRepository,
        IUserRepository userRepository
    )
    {
        _authService = authService;
        _coachRepository = coachRepository; 
        _personRepository = personRepository; 
        _userRepository  = userRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return Error("Username and password are required");

        var user = await _authService.AuthenticateAsync(request.Username, request.Password);
        
        if (user == null)
            return Error("Invalid username or password");

        var role = request.Username == "coach" ? "Coach" : "Member";

        var response = new
        {
            user.Id,
            Username = user.Person?.Username ?? request.Username,
            FirstName = user.Person?.FirstName,
            LastName = user.Person?.LastName,
            Email = user.Person?.Email,
            Role = role
        };

        return Success(response, "Login successful");
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = HttpContext.Items["UserId"] as int?;
        var username = HttpContext.Items["Username"] as string;

        if (userId == null)
            return Unauthorized();

        return Success(new
        {
            Id = userId.Value,
            Username = username
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return Error("Username and password are required");

        if (request.Password.Length < 6)
            return Error("Password must be at least 6 characters");

        // بررسی تکراری نبودن نام کاربری
        var existingPerson = await _personRepository.GetByUsernameAsync(request.Username);
        if (existingPerson != null)
            return Error("Username already exists", 409);

        // ایجاد Person جدید
        var person = new Person
        {
            Username = request.Username,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Gender = Gender.Male,
            Age = 25,
            CreatedAt = DateTime.UtcNow
        };

        var createdPerson = await _personRepository.AddAsync(person);

        if (request.Role == "Coach")
        {
            // ایجاد Coach
            var coach = new Coach
            {
                PersonId = createdPerson.Id,
                Specialization = "General",
                YearsOfExperience = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _coachRepository.AddAsync(coach);
            
            // همچنین برای مربی یک User ایجاد کن (برای ورود به سیستم)
            var user = new User
            {
                PersonId = createdPerson.Id,
                Goal = Goal.Fitness,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user);
        }
        else
        {
            // ایجاد User (Member)
            var user = new User
            {
                PersonId = createdPerson.Id,
                Goal = Goal.Fitness,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user);
        }

        return Success<object>(null, "Registration successful");
    }

}