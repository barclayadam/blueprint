using JetBrains.Annotations;

namespace Blueprint;

/// <summary>
/// Identifies a command that can be executed within the system that will modify data (i.e. update, create
/// or delete). Usually represented as a POST, PATCH or DELETE HTTP operation and, unless explicitly coded
/// for not typically safe to re-execute.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface ICommand
{
}

/// <summary>
/// An extension of <see cref="ICommand" /> that specifies the response type that will be generated by
/// the operation.
/// </summary>
/// <typeparam name="TResponse">The type of response that will be generated.</typeparam>
public interface ICommand<TResponse> : ICommand, IReturn<TResponse>
{
}