namespace GymFlow.Models.Interfaces;

/// <summary>
/// Represents an entity that can be authenticated using username and password
/// </summary>
public interface IAuthenticable
{
    string Username { get; set; }
    string Password { get; set; }
}