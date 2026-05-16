namespace GymFlow.Models.Exceptions;

/// <summary>
/// Exception thrown when the data is outside the expected date range
/// </summary>
public class DataOutOfRangeException : Exception
{
    public int RequiredWeeks { get; set; }
    public int AvailableWeeks { get; set; }
    public DateTime EarliestDate { get; set; }
    public DateTime LatestDate { get; set; }

    public DataOutOfRangeException() : base() { }

    public DataOutOfRangeException(string message) : base(message) { }

    public DataOutOfRangeException(string message, int requiredWeeks, int availableWeeks, DateTime earliestDate, DateTime latestDate)
        : base(message)
    {
        RequiredWeeks = requiredWeeks;
        AvailableWeeks = availableWeeks;
        EarliestDate = earliestDate;
        LatestDate = latestDate;
    }
}