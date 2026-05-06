namespace GymFlow.Models.Entities;

/// <summary>
/// Abstract base for any physical person (User, Coach, Admin).
/// Contains common personal information.
/// </summary>
public abstract class Person : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public float? Weight { get; set; }   // in kg
    public float? Height { get; set; }   // in cm
    public BodyType? BodyType { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}