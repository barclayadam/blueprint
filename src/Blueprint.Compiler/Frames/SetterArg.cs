using System;
using System.Reflection;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames;

/// <summary>
/// Represents the setting of a property of a class and construction
/// time.
/// </summary>
public class SetterArg
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SetterArg" /> class,
    /// setting a property using the specified <see cref="Variable" />.
    /// </summary>
    /// <param name="propertyName">The <b>exact</b> name of the property to set.</param>
    /// <param name="variable">The variable to set the property value to.</param>
    public SetterArg(string propertyName, Variable variable)
    {
        this.PropertyName = propertyName;
        this.Variable = variable;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="SetterArg" /> class,
    /// setting a property by finding a variable of the given <paramref name="propertyType" />.
    /// </summary>
    /// <param name="propertyName">The <b>exact</b> name of the property to set.</param>
    /// <param name="propertyType">The type of the property.</param>
    public SetterArg(string propertyName, Type propertyType)
    {
        this.PropertyName = propertyName;
        this.PropertyType = propertyType;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="SetterArg" /> class,
    /// setting a property by finding a variable of the property's type.
    /// </summary>
    /// <param name="property">The property to set.</param>
    public SetterArg(PropertyInfo property)
    {
        this.PropertyName = property.Name;
        this.PropertyType = property.PropertyType;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="SetterArg" /> class,
    /// setting a property to the specified <see cref="Variable" />.
    /// </summary>
    /// <param name="property">The <b>exact</b> name of the property to set.</param>
    /// <param name="variable">The variable to set the property to.</param>
    public SetterArg(PropertyInfo property, Variable variable)
    {
        this.PropertyName = property.Name;
        this.PropertyType = property.PropertyType;
        this.Variable = variable;
    }

    /// <summary>
    /// The name of the property being set.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// The type of the property being set.
    /// </summary>
    public Type PropertyType { get; }

    /// <summary>
    /// The variable that the property will be set to (may be <c>null</c> if only
    /// a type was given and no variable has been found yet).
    /// </summary>
    public Variable Variable { get; private set; }

    /// <summary>
    /// Returns the code that will, when output, be used to set the property of
    /// a class in it's initialisation block.
    /// </summary>
    /// <returns>The setting code snippet.</returns>
    public string Assignment()
    {
        return $"{this.PropertyName} = {this.Variable}";
    }

    internal void FindVariable(IMethodVariables chain)
    {
        if (this.Variable == null)
        {
            this.Variable = chain.FindVariable(this.PropertyType);
        }
    }
}