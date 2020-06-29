namespace Blueprint.Utilities
{
    /// <summary>
    /// A class can implement this CustomJsonWriter if it can provide a very specific writer
    /// to convert itself to JSON efficiently.
    /// </summary>
    public interface ICustomJsonWriter
    {
        string ToJson();
    }
}
