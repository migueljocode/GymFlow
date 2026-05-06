namespace GymFlow.Models.Entities;

/// <summary>
/// Coach who may supervise multiple users.
/// Relationship: one-to-many with User (Clients).
/// </summary>
public class Coach : Person
{
    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }

    public virtual ICollection<User>? Clients { get; set; } = new List<User>();
}