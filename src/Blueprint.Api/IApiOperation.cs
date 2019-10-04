using JetBrains.Annotations;

namespace Blueprint.Api
{
    /// <summary>
    /// A simple marker interface that is used to identify operations that can be executed in the system, an
    /// operation being defined as a simple property bag with parameters, metadata in the form of attributes, and
    /// an association <see cref="IApiOperationHandler{T}"/>.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    public interface IApiOperation
    {
    }
}
