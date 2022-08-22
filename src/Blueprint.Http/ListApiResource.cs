using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Http;

/// <summary>
/// An <see cref="ApiResource" /> that contains a list of other resources.
/// </summary>
/// <typeparam name="T">The type of the API resource represented.</typeparam>
public class ListApiResource<T> : ApiResource, IApiResourceCollection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListApiResource{T}"/> class.
    /// </summary>
    /// <param name="values">The values of this list.</param>
    public ListApiResource(IEnumerable<T> values)
    {
        this.Object = $"list.{GetTypeName(typeof(T))}";

        // NB: It's important to consume values which could be a LINQ query as otherwise modifications
        // in middleware could be lost if the values are enumerated multiple times
        var asList = values.ToList();

        this.Values = asList;
        this.Total = asList.Count;
    }

    /// <summary>
    /// The values of this list.
    /// </summary>
    public IEnumerable<T> Values { get; }

    /// <summary>
    /// The total count of values in this list.
    /// </summary>
    // Note that we use internal set to allow PagedApiResource to set this to the total of all items, not just
    // of the page.
    public long Total { get; internal set; }

    /// <inheritdoc/>
    public IEnumerable<object> GetEnumerable()
    {
        return this.Values as IEnumerable<object>;
    }
}