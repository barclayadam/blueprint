using System;
using System.Linq;
using System.Threading.Tasks;

using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler;

/// <summary>
/// A set of extensions to <see cref="ISourceWriter" /> that provides methods for common language
/// constructs.
/// </summary>
public static class SourceWriterExtensions
{
    private static readonly string _returnCompletedTask = $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";

    private static readonly string _returnFromResult =
        $"return {typeof(Task).FullName}.{nameof(Task.FromResult)}({{0}});";

    /// <summary>
    /// Adds a "namespace [@namespace]" declaration into the code, and starts a new
    /// code block with a leading '{' character.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="namespace">The namespace.</param>
    public static void Namespace(this ISourceWriter writer, string @namespace)
    {
        writer.Block($"namespace {@namespace}");
    }

    /// <summary>
    /// Adds a "using namespace;" declaration into the code for the namespace
    /// that holds the type T.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <typeparam name="T">The type from which to grab the namespace.</typeparam>
    public static void UsingNamespace<T>(this ISourceWriter writer)
    {
        writer.WriteLine($"using {typeof(T).Namespace};");
    }

    /// <summary>
    /// Adds a "using namespace;" declaration into the code for the namespace.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="namespace">The namespace.</param>
    public static void UsingNamespace(this ISourceWriter writer, string @namespace)
    {
        writer.WriteLine($"using {@namespace};");
    }

    /// <summary>
    /// Writes "using ([declaration])" into the code and starts a new code
    /// block with a leading '{' character.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="declaration">The code that goes within the parenthesis of this using block (i.e. the expression that generates and optionally sets, the disposable object).</param>
    /// <param name="inner">The action that writes the body of the using block, passed the same writer to avoid closure allocation.</param>
    public static void Using(this ISourceWriter writer, string declaration, Action<ISourceWriter> inner)
    {
        writer.Block($"using ({declaration})");
        inner(writer);
        writer.FinishBlock();
    }

    /// <summary>
    /// Writes "try ([declaration])" into the code and starts a new code
    /// block with a leading '{' character.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="inner">The action that writes the body of the try block, passed the same writer to avoid closure allocation.</param>
    public static void Try(
        this ISourceWriter writer,
        Action<ISourceWriter> inner)
    {
        writer.Block("try");
        inner(writer);
        writer.FinishBlock();
    }

    /// <summary>
    /// Writes "catch ([declaration])" into the code and starts a new code
    /// block with a leading '{' character.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="declaration">The code that goes within the parenthesis of this catch block (i.e. at a minimum the exception type).</param>
    /// <param name="inner">The action that writes the body of the catch block, passed the same writer to avoid closure allocation.</param>
    public static void Catch(
        this ISourceWriter writer,
        string declaration,
        Action<ISourceWriter> inner)
    {
        writer.Block("catch (" + declaration + ")");
        inner(writer);
        writer.FinishBlock();
    }

    /// <summary>
    /// Writes either "return;" or "return Task.CompletedTask;" into the code
    /// for synchronous or asynchronous methods.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="method">The method from which we are returning.</param>
    public static void Return(this ISourceWriter writer, GeneratedMethod method)
    {
        if (method.AsyncMode == AsyncMode.AsyncTask)
        {
            writer.WriteLine("return;");
        }
        else
        {
            writer.WriteLine(_returnCompletedTask);
        }
    }

    /// <summary>
    /// Writes a "return [variable.Usage];" code snippet .
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="method">The method the return statement belongs to.</param>
    /// <param name="variable">The variable to return.</param>
    public static void Return(this ISourceWriter writer, GeneratedMethod method, Variable variable)
    {
        writer.WriteLine(method.AsyncMode == AsyncMode.AsyncTask
            ? $"return {variable.Usage};"
            : string.Format(_returnFromResult, variable.Usage));
    }

    /// <summary>
    /// Writes the text into the code as a comment at the current
    /// block level.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="comment">The comment text (must not contain a newline).</param>
    public static void Comment(this ISourceWriter writer, string comment)
    {
        writer.WriteLine("// " + comment);
    }

    /// <summary>
    /// Starts an if block in code with the opening brace and indention for following lines.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="statement">The statement to put inside the if block.</param>
    public static void If(this ISourceWriter writer, string statement)
    {
        writer.Block($"if ({statement})");
    }

    /// <summary>
    /// Starts an else block in code with the opening brace and indention for following lines.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    public static void Else(this ISourceWriter writer)
    {
        writer.Block("else");
    }

    /// <summary>
    /// Starts a try block in code with the opening brace and indention for following lines.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    public static void Try(this ISourceWriter writer)
    {
        writer.Block("try");
    }

    /// <summary>
    /// Starts a finally block in code with the opening brace and indention for following lines.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    public static void Finally(this ISourceWriter writer)
    {
        writer.FinishBlock();
        writer.Block("finally");
    }

    /// <summary>
    /// Writes the declaration of a new class to the source writer.
    /// </summary>
    /// <param name="writer">Where to write to.</param>
    /// <param name="className">The name of the class.</param>
    /// <param name="inheritsOrImplements">The set of <see cref="Type" />s that the class either inherits from, or implements.</param>
    public static void StartClass(this ISourceWriter writer, string className, params Type[] inheritsOrImplements)
    {
        if (inheritsOrImplements.Length == 0)
        {
            writer.Block($"public class {className}");
        }
        else
        {
            var tempQualifier = inheritsOrImplements.Select(x => x.FullNameInCode());
            writer.Block($"public class {className} : {string.Join(", ", tempQualifier)}");
        }
    }
}