using System.Collections.Generic;

namespace Blueprint.Tests;

/// <summary>
/// A utility class used in XUnit theories that enable the simple wrapping of a data object with a name that will be used
/// in the test name.
/// </summary>
/// <remarks>
/// Using this allows for the easy and explicit naming of theory test cases. The test should take a single parameter of this
/// class and wrap the data it needs inside (i.e. if multiple params are required either create an enclosing class or mimic
/// XUnit's defaults and make <typeparamref name="T" /> an <see cref="IEnumerable{T}" />.
/// </remarks>
/// <typeparam name="T">The data type to wrap.</typeparam>
public class Labelled<T>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="Labelled{T}" /> class.
    /// </summary>
    /// <param name="name">The test case name,</param>
    /// <param name="data">The wrapped data.</param>
    public Labelled(string name, T data)
    {
        this.Name = name;
        this.Data = data;
    }

    /// <summary>
    /// The name of the tets case.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The data that has been wrapped.
    /// </summary>
    public T Data { get; }

    public static implicit operator T(Labelled<T> labelled)
    {
        return labelled.Data;
    }

    /// <summary>
    /// Returns the <see cref="Name" /> property.
    /// </summary>
    /// <returns>The <see cref="Name" /> property.</returns>
    public override string ToString()
    {
        return this.Name;
    }
}