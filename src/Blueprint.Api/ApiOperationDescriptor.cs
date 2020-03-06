using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Blueprint.Api.Authorisation;
using Blueprint.Core;
using Blueprint.Core.Utilities;

namespace Blueprint.Api
{
    /// <summary>
    /// A descriptor of an <see cref="IApiOperation" />, containing details such as the URL from
    /// which the operation can be executed, and the type that represents the actual operation.
    /// </summary>
    [DebuggerVisualizer(nameof(Name))]
    public class ApiOperationDescriptor
    {
        private readonly Dictionary<string, object> featureData = new Dictionary<string, object>();

        /// <summary>
        /// Initialises a new instance of the <see cref="ApiOperationDescriptor" /> class.
        /// </summary>
        /// <param name="apiOperationType">The operation type (must implement <see cref="IApiOperation"/>).</param>
        public ApiOperationDescriptor(Type apiOperationType)
        {
            Guard.NotNull(nameof(apiOperationType), apiOperationType);

            OperationType = apiOperationType;

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
        /// Gets or sets the name of the operation, which is the class name of the operation with Query and Command removed.
        /// </summary>
        public string Name { get; set; }

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
        /// Gets a value indicating whether this operation describes an <see cref="ICommand" />.
        /// </summary>
        public bool IsCommand => typeof(ICommand).IsAssignableFrom(OperationType);

        /// <summary>
        /// Gets the feature data of the specified type, which provides a pluggable mechanism to add additional
        /// structured data to <see cref="ApiOperationDescriptor" />s, such as HTTP-related data.
        /// </summary>
        /// <typeparam name="T">The feature data type to load.</typeparam>
        /// <returns>The feature data of the specified type.</returns>
        /// <exception cref="InvalidOperationException">If no such feature data exists.</exception>
        public T GetFeatureData<T>()
        {
            var key = typeof(T).FullName;

            if (featureData.TryGetValue(key, out var data))
            {
                return (T)data;
            }

            throw new InvalidOperationException(
                $"Could not find feature data {key}");
        }

        /// <summary>
        /// Adds the given feature data to this descriptor.
        /// </summary>
        /// <param name="newFeatureData">The feature data to store.</param>
        public void SetFeatureData(object newFeatureData)
        {
            Guard.NotNull(nameof(featureData), featureData);

            var key = newFeatureData.GetType().FullName;

            featureData[key] = newFeatureData;
        }

        /// <summary>
        /// Creates a new instance of the API operation this descriptor describes, with the default implementation
        /// to simply be to use <see cref="Activator"/>, which means operations must have a public, parameterless
        /// constructor.
        /// </summary>
        /// <returns>A new instance of operation this descriptor describes.</returns>
        public IApiOperation CreateInstance()
        {
            return (IApiOperation)Activator.CreateInstance(OperationType);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return OperationType.FullName;
        }
    }
}
