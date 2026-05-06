namespace GymFlow.Models.Base;

/// <summary>
/// Base class for all entities, providing common primary key and audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Unique identifier (primary key).</summary>
    public int Id { get; set; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC last update timestamp (null if never updated).</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag. When true, entity is considered deleted but remains in database.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Timestamp when entity was soft-deleted (null if not deleted).
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
