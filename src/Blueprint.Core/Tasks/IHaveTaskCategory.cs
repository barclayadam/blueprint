using Newtonsoft.Json;

namespace Blueprint.Core.Tasks
{
    /// <summary>
    /// An interface that allows the categorisation of tasks based on their runtime data, in addition
    /// to their static type.
    /// </summary>
    public interface IHaveTaskCategory
    {
        [JsonIgnore]
        string Category { get; }
    }
}