using System;

namespace Blueprint.Compiler.Model;

/// <summary>
/// Represents a source of variables, which allows for variables to be "created" on demand to,
/// for example implement a means of creating variables from a DI container, or to find
/// variables from the "context" of methods.
/// </summary>
public interface IVariableSource
{
    /// <summary>
    /// Tries to find a variable, being given access to the variables from a method plus
    /// the type that
    /// </summary>
    /// <param name="variables">The source of variables for the method from which we are trying to find a variable.</param>
    /// <param name="type">The ype of variable to find.</param>
    /// <returns>A <see cref="Variable" /> if one can be found.</returns>
    Variable? TryFindVariable(IMethodVariables variables, Type type);
}