using Microsoft.AspNetCore.Mvc;
using GymFlow.Api.Controllers.Base;
using GymFlow.Services.Interfaces;

namespace GymFlow.Api.Controllers;

[Tags("Test")]
public class TestAuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public TestAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("check-user/{username}/{password}")]
    public async Task<IActionResult> CheckUserAsync(string username, string password)
    {
        var user = await _authService.AuthenticateAsync(username, password);
        
        if (user == null)
            return Error("User not found or password incorrect");
        
        return Success(new
        {
            user.Id,
            Username = user.Person?.Username,
            PasswordFromDb = user.Person?.Password,
            InputPassword = password,
            FirstName = user.Person?.FirstName,
            LastName = user.Person?.LastName
        });
    }
}