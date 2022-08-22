using System;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model;

/// <summary>
/// A <see cref="Variable" /> that casts another variable to a different type.
/// </summary>
public class CastVariable : Variable
{
    /// <summary>
    /// Initialises a new instance of the <see cref="CastVariable" /> class.
    /// </summary>
    /// <param name="parent">The variable to be cast.</param>
    /// <param name="specificType">The type to cast this variable to.</param>
    public CastVariable(Variable parent, Type specificType)
        : base(specificType, $"(({specificType.FullNameInCode()}){parent})")
    {
        this.Dependencies.Add(parent);
        this.Inner = parent;
    }

    /// <summary>
    /// The variable that is being cast.
    /// </summary>
    public Variable Inner { get; }
}