using JetBrains.Annotations;

namespace Blueprint;

/// <summary>
/// Identifies a query that can be executed within the system to read data from it, the
/// read side of a CQRS system.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IQuery
{
}

/// <summary>
/// An extension of <see cref="IQuery" /> that specifies the response type that will be generated by
/// the operation.
/// </summary>
/// <typeparam name="TResponse">The type of response that will be generated.</typeparam>
public interface IQuery<TResponse> : IQuery, IReturn<TResponse>
{
}