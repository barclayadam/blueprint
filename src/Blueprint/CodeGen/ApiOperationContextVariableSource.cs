using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using Blueprint.Authorisation;
using Blueprint.Compiler.Model;

namespace Blueprint.CodeGen
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
    ///  * The specific type of IApiOperation for the context. Casted from from <see cref="ApiOperationContext.Operation" />.
    ///  * <see cref="IServiceProvider" /> from <see cref="ApiOperationContext.ServiceProvider" />.
    ///  * <see cref="IUserAuthorisationContext" /> from <see cref="ApiOperationContext.UserAuthorisationContext" />.
    ///  * <see cref="Activity" /> from <see cref="ApiOperationContext.Activity" />.
    ///  * <see cref="ClaimsIdentity" /> from <see cref="ApiOperationContext.ClaimsIdentity" />.
    /// </remarks>
    public class ApiOperationContextVariableSource : IVariableSource
    {
        private readonly Argument _operationContextVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiOperationContextVariableSource" /> class.
        /// </summary>
        /// <param name="operationContextVariable">The <see cref="ApiOperationContext"/> variable of the method.</param>
        /// <param name="castFrameCastOperationVariable">The variable representing the operation as it's actual
        /// type.</param>
        public ApiOperationContextVariableSource(Argument operationContextVariable, Variable castFrameCastOperationVariable)
        {
            this._operationContextVariable = operationContextVariable;

            this.OperationVariable = castFrameCastOperationVariable;
        }

        /// <summary>
        /// Gets the variable that represents the actual property for this context, casted from the <see cref="ApiOperationContext.Operation"/> property.
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
            return this.DoTryFindVariable(type) ??
                   throw new ArgumentException($"{nameof(ApiOperationContextVariableSource)} cannot build variable of type {type.Name}");
        }

        /// <inheritdoc />
        public Variable TryFindVariable(IMethodVariables variables, Type type)
        {
            return this.DoTryFindVariable(type);
        }

        private Variable DoTryFindVariable(Type type)
        {
            if (type == typeof(ApiDataModel))
            {
                return this._operationContextVariable.GetProperty(nameof(ApiOperationContext.DataModel));
            }

            if (type == typeof(ApiOperationDescriptor))
            {
                return this._operationContextVariable.GetProperty(nameof(ApiOperationContext.Descriptor));
            }

            if (type == typeof(ApiOperationContext))
            {
                return this._operationContextVariable;
            }

            if (type == this.OperationVariable.VariableType)
            {
                return this.OperationVariable;
            }

            if (type == typeof(IServiceProvider))
            {
                return this._operationContextVariable.GetProperty(nameof(ApiOperationContext.ServiceProvider));
            }

            if (type == typeof(IUserAuthorisationContext))
            {
                return this._operationContextVariable.GetProperty(nameof(ApiOperationContext.UserAuthorisationContext));
            }

            if (type == typeof(ClaimsIdentity))
            {
                return this._operationContextVariable.GetProperty(nameof(ApiOperationContext.ClaimsIdentity));
            }

            if (type == typeof(Activity))
            {
                return this._operationContextVariable.GetProperty(nameof(ApiOperationContext.Activity));
            }

            if (type == typeof(CancellationToken))
            {
                return this._operationContextVariable.GetProperty(nameof(ApiOperationContext.OperationCancelled));
            }

            return null;
        }
    }
}
