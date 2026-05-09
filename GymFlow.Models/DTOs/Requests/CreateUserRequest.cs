namespace GymFlow.Models.DTOs.Requests;

public class CreateUserRequest
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [Phone]
    [MaxLength(15)]
    public string? Phone { get; set; }
    
    public Gender Gender { get; set; }
    
    [Range(1, 120)]
    public int Age { get; set; }
    
    [Range(30, 250)]
    public float? Weight { get; set; }
    
    [Range(100, 250)]
    public float? Height { get; set; }
    
    public BodyType? BodyType { get; set; }
    public Goal Goal { get; set; }
    
    [Range(1000, 5000)]
    public int? EstimatedCaloriesIntake { get; set; }
    
    public bool IsCompetitive { get; set; }
}