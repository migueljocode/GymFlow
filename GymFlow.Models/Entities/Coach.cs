using GymFlow.Models.Base;

namespace GymFlow.Models.Entities;

public class Coach : BaseEntity
{
    public int PersonId { get; set; }
    public virtual Person Person { get; set; } = null!;
    
    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? CertificateUrl { get; set; }
    
    // مربی می‌تواند چندین کاربر داشته باشد
    public virtual ICollection<User> Clients { get; set; } = new List<User>();
}