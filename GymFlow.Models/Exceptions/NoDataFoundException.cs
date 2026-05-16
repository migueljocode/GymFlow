namespace GymFlow.Models.Exceptions;

/// <summary>
/// Exception thrown when no data exists for the requested operation
/// </summary>
public class NoDataFoundException : Exception
{
    public string EntityName { get; set; }
    public string? FilterInfo { get; set; }

    public NoDataFoundException() : base()
    {
        EntityName = string.Empty;
    }

    public NoDataFoundException(string message) : base(message)
    {
        EntityName = string.Empty;
    }

    public NoDataFoundException(string message, string entityName, string? filterInfo = null) 
        : base(message)
    {
        EntityName = entityName ?? string.Empty;
        FilterInfo = filterInfo;
    }

    public NoDataFoundException(string message, Exception innerException) 
        : base(message, innerException)
    {
        EntityName = string.Empty;
    }
}