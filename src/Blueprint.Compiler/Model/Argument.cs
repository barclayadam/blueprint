using System;
using System.Reflection;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model;

/// <summary>
/// Variable that represents the input argument to a generated method.
/// </summary>
public class Argument : Variable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Argument"/> class.
    /// </summary>
    /// <param name="variableType">The type of the variable.</param>
    /// <param name="usage">The "usage" of the variable, which is typically the name of the variable/argument.</param>
    public Argument(Type variableType, string usage)
        : base(variableType, usage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Argument"/> class from a known <see cref="ParameterInfo" />.
    /// </summary>
    /// <param name="parameter">The parameter this argument variable represents.</param>
    public Argument(ParameterInfo parameter)
        : this(parameter.ParameterType, parameter.Name)
    {
    }

    /// <summary>
    /// Gets the string declaration of this <see cref="Argument" />, the text that would be written in the method's parameter
    /// list.
    /// </summary>
    public string Declaration => $"{this.VariableType.FullNameInCode()} {this.Usage}";

    /// <summary>
    /// Creates a <see cref="Argument" /> of the given type <typeparamref name="T" />
    /// </summary>
    /// <param name="argName">The name of the argument, generating one from the type if not specified.</param>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <returns>A new <see cref="Argument" />.</returns>
    public static new Argument For<T>(string argName = null)
    {
        return new Argument(typeof(T), argName ?? DefaultName(typeof(T)));
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((Argument)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return ((this.VariableType != null ? this.VariableType.GetHashCode() : 0) * 397) ^ (this.Usage != null ? this.Usage.GetHashCode() : 0);
        }
    }

    private bool Equals(Argument other)
    {
        return this.VariableType == other.VariableType && string.Equals(this.Usage, other.Usage);
    }
}