using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Blueprint.Api.Authorisation;
using Blueprint.Core;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Utilities;

namespace Blueprint.Api
{
    /// <summary>
    /// A descriptor of an <see cref="IApiOperation" />, containing details such as the URL from
    /// which the operation can be executed, and the type that represents the actual operation.
    /// </summary>
    public class ApiOperationDescriptor
    {
        public ApiOperationDescriptor(Type apiOperationType, HttpMethod httpMethod)
        {
            Guard.NotNull(nameof(apiOperationType), apiOperationType);
            Guard.NotNull(nameof(httpMethod), httpMethod);

            OperationType = apiOperationType;
            HttpMethod = httpMethod;

            TypeAttributes = apiOperationType.GetCustomAttributes(true);
            Properties = apiOperationType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyAttributes = Properties.Select(p => p.GetCustomAttributes(true)).ToArray();

            AnonymousAccessAllowed = false;
            IsExposed = true;
            ShouldAudit = true;
            RecordPerformanceMetrics = true;

            Name = apiOperationType.Name.Replace("Query", string.Empty).Replace("Command", string.Empty).ToPascalCase();
        }

        /// <summary>
        /// Gets the name of the operation, which is the class name of the operation with Query and Command removed.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type that represents this operation, a type that defines the parameters required (or
        /// optional parameters), plus any other metadata that is used by middleware components to
        /// decide what action to take (for example any declarative permissions in the form of
        /// attributes).
        /// </summary>
        public Type OperationType { get; }

        /// <summary>
        /// Gets the custom attributes that have been applied to the type, stored once to avoid having to
        /// re-query the type system whenever extra information is required.
        /// </summary>
        public object[] TypeAttributes { get; }

        /// <summary>
        /// Gets the public properties declared for the operation type, stored once to avoid having to
        /// re-query the type system whenever extra information is required.
        /// </summary>
        public PropertyInfo[] Properties { get; }

        /// <summary>
        /// Gets the public properties attributes, indexed such that the attributes for Properties[i] can be
        /// found at PropertyAttributes[i].
        /// </summary>
        public object[][] PropertyAttributes { get; }

        /// <summary>
        /// Gets the HTTP method that this operation is required to be executed using, i.e. one of GET, POST,
        /// PUT, DELETE.
        /// </summary>
        public HttpMethod HttpMethod { get; }

        /// <summary>
        /// Gets or sets a value indicating whether anonymous access is allowed to this API operation, typically determined
        /// by the presence of an <see cref="AllowAnonymousAttribute"/> decorating the operation.
        /// </summary>
        public bool AnonymousAccessAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this operation is exposed, that it will be included in the generation
        /// of links to attach to resource results, defaults to <c>true</c>.
        /// </summary>
        /// <remarks>
        /// This value is typically determined conventionally by looking for an <see cref="UnexposedOperationAttribute"/>, which
        /// its presence setting this value to <c>false</c>.
        /// </remarks>
        public bool IsExposed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this operation should be audited, which will also be used to
        /// determine the level of logging &amp; detail of logging that occurs, defaults to <c>true</c>.
        /// </summary>
        public bool ShouldAudit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether performance metrics should be recorded for this operation.
        /// </summary>
        /// <remarks>
        /// This value should typically be <c>true</c>, the default value, although in some cases the performance metrics
        /// can be noise (e.g. for a health endpoint that is hit every minute).
        /// </remarks>
        public bool RecordPerformanceMetrics { get; set; }

        /// <summary>
        /// Creates a new instance of the API operation this descriptor describes, with the default implementation
        /// to simply be to use <see cref="Activator"/>, which means operations must have a public, parameterless
        /// constructor.
        /// </summary>
        /// <returns>A new instance of operation this descriptor describes.</returns>
        public virtual IApiOperation CreateInstance()
        {
            return (IApiOperation)Activator.CreateInstance(OperationType);
        }
    }
}
