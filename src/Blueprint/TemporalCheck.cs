namespace Blueprint;

/// <summary>
/// Used with the four different time based attributes. This is used to specify weather the Date or DateTime
/// should be used for checking where the provided date falls.
/// </summary>
public enum TemporalCheck
{
    /// <summary>
    /// Only look at the Date part of a DateTime.
    /// </summary>
    Date,

    /// <summary>
    /// Use the whole DateTime.
    /// </summary>
    DateTime,
}