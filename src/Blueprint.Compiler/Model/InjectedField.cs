using System;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model;

/// <summary>
/// Represents a <see cref="Variable" /> that will be passed in to
/// a type via the constructor.
/// </summary>
public class InjectedField : Variable
{
    /// <summary>
    /// Initialises a new instance of the <see cref="InjectedField" /> class
    /// with the given argument type and a default generated name.
    /// </summary>
    /// <param name="argType">The type of the field that will be injected through
    /// the constructor.</param>
    public InjectedField(Type argType)
        : this(argType, DefaultName(argType))
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="InjectedField" /> class
    /// with the given argument type and name.
    /// </summary>
    /// <param name="argType">The type of the field that will be injected through
    /// the constructor.</param>
    /// <param name="name">The name of the field to be injected.</param>
    public InjectedField(Type argType, string name)
        : base(argType, "_" + name)
    {
        this.ArgumentName = name;
    }

    /// <summary>
    /// The name of the argument.
    /// </summary>
    public string ArgumentName { get; }

    /// <summary>
    /// The declaration of the argument in the constructor's parameter
    /// list.
    /// </summary>
    public virtual string CtorArgDeclaration => $"{this.VariableType.FullNameInCode()} {this.ArgumentName}";

    /// <summary>
    /// Writes the field declaration to hold this variable in the class.
    /// </summary>
    /// <param name="writer">The writer to write the declaration to.</param>
    public void WriteDeclaration(ISourceWriter writer)
    {
        writer.WriteLine($"private readonly {this.VariableType.FullNameInCode()} {this.Usage};");
    }

    /// <summary>
    /// Writes the assignment (i.e. the code snippet to be placed inside the
    /// constructors' body to set the field from the argument).
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    public virtual void WriteAssignment(ISourceWriter writer)
    {
        writer.WriteLine($"{this.Usage} = {this.ArgumentName};");
    }
}