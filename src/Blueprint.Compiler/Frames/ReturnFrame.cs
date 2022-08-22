using System;
using System.Threading.Tasks;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Frames;

/// <summary>
/// A simple <see cref="Frame" /> that represents a <c>return</c> expression.
/// </summary>
public class ReturnFrame : SyncFrame
{
    private readonly Type _returnType;

    /// <summary>
    /// Initialises a new instance of the <see cref="ReturnFrame" /> that
    /// has no return type / variable (i.e. a  <c>void</c> method).
    /// </summary>
    public ReturnFrame()
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ReturnFrame" /> that
    /// has the specified return type.
    /// </summary>
    /// <param name="returnType">The type of return for this frame, which should
    /// match that of the parent <see cref="GeneratedMethod" />.</param>
    public ReturnFrame(Type returnType)
    {
        this._returnType = returnType;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ReturnFrame" /> that
    /// returns the given <see cref="Variable" />.
    /// </summary>
    /// <param name="returnVariable">The variable to return, which should
    /// have a type matcing that of the parent <see cref="GeneratedMethod" />.
    /// </param>
    public ReturnFrame(Variable returnVariable)
    {
        this.ReturnedVariable = returnVariable;
    }

    /// <summary>
    /// The returned <see cref="Variable" />, if any (i.e. if not a <c>void</c>
    /// method).
    /// </summary>
    public Variable ReturnedVariable { get; private set; }

    /// <inheritdoc />
    protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
    {
        if (this.ReturnedVariable == null && this._returnType != null)
        {
            this.ReturnedVariable = variables.TryFindVariable(this._returnType);
        }

        if (this.ReturnedVariable == null)
        {
            writer.WriteLine("return;");
        }
        else
        {
            var variableIsTask = this.ReturnedVariable.VariableType.IsGenericType && this.ReturnedVariable.VariableType.GetGenericTypeDefinition() == typeof(Task<>);
            var methodReturnsTask = method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

            // This method does not use async/await but _does_ return Task, but the variable to return is _not_ a Task<>, therefore we
            // need to use Task.FromResult to get the correct return type
            if (method.AsyncMode == AsyncMode.None && methodReturnsTask && !variableIsTask)
            {
                // What type are we expecting to return?
                var taskValueType = method.ReturnType.GenericTypeArguments[0];

                writer.WriteLine(
                    $"return {typeof(Task).FullNameInCode()}.{nameof(Task.FromResult)}(({taskValueType.FullNameInCode()}){this.ReturnedVariable});");
            }
            else
            {
                writer.WriteLine($"return {this.ReturnedVariable};");
            }
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.ReturnedVariable == null ? "return;" : $"return {this.ReturnedVariable};";
    }
}