using System;
using System.Security.Claims;
using System.Threading;
using Blueprint.Compiler.Model;
using Blueprint.Core.Authorisation;

namespace Blueprint.Api.CodeGen
{
    /// <summary>
    /// An <see cref="IVariableSource" /> that can creates variables for the major properties of an
    /// <see cref="ApiOperationContext" /> that has been previously registered.
    /// </summary>
    /// <remarks>
    /// The given properties can be matched:
    ///
    ///  * <see cref="ApiDataModel" /> from <see cref="ApiOperationContext.DataModel" />.
    ///  * <see cref="ApiOperationDescriptor" /> from <see cref="ApiOperationContext.Descriptor" />.
    ///  * <see cref="IApiOperation" /> from <see cref="ApiOperationContext.Operation" />.
    ///  * The specific type of IApiOperation for the context. Casted from from <see cref="ApiOperationContext.Operation" />.
    ///  * <see cref="IServiceProvider" /> from <see cref="ApiOperationContext.ServiceProvider" />.
    ///  * <see cref="IUserAuthorisationContext" /> from <see cref="ApiOperationContext.UserAuthorisationContext" />.
    ///  * <see cref="ClaimsIdentity" /> from <see cref="ApiOperationContext.ClaimsIdentity" />.
    /// </remarks>
    public class ApiOperationContextVariableSource : IVariableSource
    {
        private readonly Argument operationContextVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiOperationContextVariableSource" /> class.
        /// </summary>
        /// <param name="operationContextVariable">The <see cref="ApiOperationContext"/> variable of the method.</param>
        /// <param name="castFrameCastOperationVariable">The variable representing the <see cref="IApiOperation"/> as it's actual
        /// type.</param>
        public ApiOperationContextVariableSource(Argument operationContextVariable, Variable castFrameCastOperationVariable)
        {
            this.operationContextVariable = operationContextVariable;

            OperationVariable = castFrameCastOperationVariable;
        }

        /// <summary>
        /// Gets the variable that represents the actual property for this context, casted from the <see cref="IApiOperation" /> property
        /// of the <see cref="ApiOperationContext" />.
        /// </summary>
        public Variable OperationVariable
        {
            get;
        }

        /// <summary>
        /// Gets a <see cref="Variable" /> that represents access to a property of an <see cref="ApiOperationContext" />.
        /// </summary>
        /// <param name="type">The type of variable requested.</param>
        /// <returns>The corresponding <see cref="Variable"/> for the type.</returns>
        public Variable Get(Type type)
        {
            return DoTryFindVariable(type) ??
                   throw new ArgumentException($"{nameof(ApiOperationContextVariableSource)} cannot build variable of type {type.Name}");
        }

        /// <inheritdoc />
        public Variable TryFindVariable(IMethodVariables variables, Type type)
        {
            return DoTryFindVariable(type);
        }

        private Variable DoTryFindVariable(Type type)
        {
            if (type == typeof(ApiDataModel))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.DataModel));
            }

            if (type == typeof(ApiOperationDescriptor))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.Descriptor));
            }

            if (type == typeof(ApiOperationContext))
            {
                return operationContextVariable;
            }

            if (type == typeof(IApiOperation))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.Operation));
            }

            if (type == typeof(IServiceProvider))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.ServiceProvider));
            }

            if (type == typeof(IUserAuthorisationContext))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.UserAuthorisationContext));
            }

            if (type == typeof(ClaimsIdentity))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.ClaimsIdentity));
            }

            if (type == typeof(CancellationToken))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.OperationCancelled));
            }

            if (type == OperationVariable.VariableType)
            {
                return OperationVariable;
            }

            return null;
        }
    }
}
