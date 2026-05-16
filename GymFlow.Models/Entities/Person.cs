using GymFlow.Models.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace GymFlow.Models.Entities;

public class Person : BaseEntity, IAuthenticable
{
    // اطلاعات شخصی
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public float? Weight { get; set; }
    public float? Height { get; set; }
    public BodyType? BodyType { get; set; }
    
    // اطلاعات ورود (Authentication) - پیاده‌سازی از IAuthenticableu
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    // روابط
    public virtual User? User { get; set; }
    public virtual Coach? Coach { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}