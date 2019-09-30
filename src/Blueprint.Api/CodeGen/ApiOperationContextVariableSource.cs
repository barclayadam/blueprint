using System;
using System.Reflection;
using System.Security.Claims;
using Blueprint.Compiler.Model;
using Blueprint.Core.Authorisation;
using Microsoft.AspNetCore.Http;

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
    ///  * <see cref="HttpRequest" /> from <see cref="ApiOperationContext.Request" />.
    ///  * <see cref="HttpResponse" /> from <see cref="ApiOperationContext.Response" />.
    ///  * <see cref="ClaimsIdentity" /> from <see cref="ApiOperationContext.ClaimsIdentity" />.
    /// </remarks>
    public class ApiOperationContextVariableSource : IVariableSource
    {
        private readonly Argument operationContextVariable;

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
            return ((IVariableSource)this).Create(type);
        }

        public Variable GetOperationProperty(PropertyInfo property)
        {
            var operationPropertyVariable = new Variable(property.PropertyType, $"{OperationVariable}.{property.Name}");
            operationPropertyVariable.Dependencies.Add(OperationVariable);

            return operationPropertyVariable;
        }

        bool IVariableSource.Matches(Type type)
        {
            return type == typeof(ApiDataModel) ||
                   type == typeof(ApiOperationDescriptor) ||
                   type == typeof(IApiOperation) ||
                   type == typeof(IServiceProvider) ||
                   type == typeof(IUserAuthorisationContext) ||
                   type == typeof(HttpContext) ||
                   type == typeof(HttpRequest) ||
                   type == typeof(HttpResponse) ||
                   type == typeof(ClaimsIdentity) ||
                   type == OperationVariable.VariableType;
        }

        Variable IVariableSource.Create(Type type)
        {
            if (type == typeof(ApiDataModel))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.DataModel));
            }

            if (type == typeof(ApiOperationDescriptor))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.Descriptor));
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

            if (type == typeof(HttpContext))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.HttpContext));
            }

            if (type == typeof(HttpRequest))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.Request));
            }

            if (type == typeof(HttpResponse))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.Response));
            }

            if (type == typeof(ClaimsIdentity))
            {
                return operationContextVariable.GetProperty(nameof(ApiOperationContext.ClaimsIdentity));
            }

            if (type == OperationVariable.VariableType)
            {
                return OperationVariable;
            }

            throw new InvalidOperationException($"Cannot create variable of type {type.FullName}");
        }
    }
}
