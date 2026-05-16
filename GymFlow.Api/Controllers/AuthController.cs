using Microsoft.AspNetCore.Mvc;
using GymFlow.Api.Controllers.Base;
using GymFlow.Services.Interfaces;
using GymFlow.Models.DTOs.Requests;
using GymFlow.Models.Entities;

namespace GymFlow.Api.Controllers;

[Tags("Authentication")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
}