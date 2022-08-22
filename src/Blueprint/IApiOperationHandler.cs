using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Blueprint;

/// <summary>
/// A handler for a specific operation identified by the type parameter.
/// </summary>
/// <typeparam name="T">The type of API operation this component handles.</typeparam>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public interface IApiOperationHandler<in T>
{
    /// <summary>
    /// This method will be invoked with an operation of type <typeparamref name="T"/>
    /// </summary>
    /// <remarks>
    /// If this operation handler is async then it is expected to use the <c>async</c> keyword and
    /// allow the compiler to return a constructed and managed <see cref="Task{T}" />. In the case of
    /// a sync method the handler can <c>return default</c> for no response, or <c>return new ValueTask(...)</c> for
    /// the result.
    /// </remarks>
    /// <param name="operation">The operation to handle.</param>
    /// <param name="apiOperationContext">The context built up for this operation's handling.</param>
    /// <returns>The result of executing this operation, which may be <c>null</c>.</returns>
    ValueTask<object> Handle(T operation, ApiOperationContext apiOperationContext);
}