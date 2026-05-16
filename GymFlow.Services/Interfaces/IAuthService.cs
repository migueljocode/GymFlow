using GymFlow.Models.Entities;

namespace GymFlow.Services.Interfaces;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password);
}