namespace GymFlow.Models.Entities;

public class Coach : BaseEntity
{
    public int PersonId { get; set; }
    public virtual Person Person { get; set; } = null!;
    
    // اطلاعات مخصوص مربی
    public string Specialization { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? CertificateUrl { get; set; }
    
    // مربی می‌تواند چندین کاربر را هدایت کند (اختیاری برای آینده)
    public virtual ICollection<User>? Clients { get; set; }
}