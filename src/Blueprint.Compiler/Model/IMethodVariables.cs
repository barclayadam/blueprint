using System;

namespace Blueprint.Compiler.Model;

/// <summary>
/// Models a logical method and how to find candidate variables.
/// </summary>
public interface IMethodVariables
{
    /// <summary>
    /// Find or create a variable with the supplied type, throwing an exception if that is not possible.
    /// </summary>
    /// <param name="type">The type of the variable to find.</param>
    /// <returns>The <see cref="Variable" /> of the specified type.</returns>
    Variable FindVariable(Type type);

    /// <summary>
    /// Try to find a variable by type and variable source. Use this when
    /// you need to differentiate between variables that are resolved
    /// from an IoC container and all other kinds of variables.
    /// </summary>
    /// <param name="type">The type of the variable to find.</param>
    /// <returns>The <see cref="Variable" /> of the specified type, or <c>null</c>.</returns>
    Variable TryFindVariable(Type type);
}