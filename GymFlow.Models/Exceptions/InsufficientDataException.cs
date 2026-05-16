namespace GymFlow.Models.Exceptions;

/// <summary>
/// Exception thrown when there is not enough data to perform an analysis or prediction
/// </summary>
public class InsufficientDataException : Exception
{
    public InsufficientDataException() : base() { }

    public InsufficientDataException(string message) : base(message) { }

    public InsufficientDataException(string message, Exception innerException) 
        : base(message, innerException) { }
}