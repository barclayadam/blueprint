using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Blueprint.Authorisation;
using Blueprint.Middleware;
using Blueprint.Utilities;

namespace Blueprint
{
    /// <summary>
    /// A descriptor of an operation, containing details such as the URL from
    /// which the operation can be executed, and the type that represents the actual operation.
    /// </summary>
    [DebuggerVisualizer(nameof(Name))]
    public class ApiOperationDescriptor
    {
        private readonly Dictionary<string, object> _featureData = new Dictionary<string, object>();
        private readonly List<ApiOperationLink> _links = new List<ApiOperationLink>();
        private readonly List<ResponseDescriptor> _responses = new List<ResponseDescriptor>();
        private readonly List<IOperationExecutorBuilder> _handlers = new List<IOperationExecutorBuilder>();

        /// <summary>
        /// Initialises a new instance of the <see cref="ApiOperationDescriptor" /> class.
        /// </summary>
        /// <param name="apiOperationType">The operation type.</param>
        /// <param name="source">The source of this operation descriptor. Useful for determining _how_ an operation
        /// has found (i.e. scan vs explicit).</param>
        public ApiOperationDescriptor(Type apiOperationType, string source)
        {
            Guard.NotNull(nameof(apiOperationType), apiOperationType);

            this.OperationType = apiOperationType;
            this.Source = source;

            this.TypeAttributes = apiOperationType.GetCustomAttributes(true);
            this.Properties = apiOperationType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            this.PropertyAttributes = this.Properties.Select(p => p.GetCustomAttributes(true)).ToArray();

            this.AnonymousAccessAllowed = false;
            this.IsExposed = true;
            this.ShouldAudit = true;
            this.RecordPerformanceMetrics = true;

            this.RequiresReturnValue = true;
            this.AllowMultipleHandlers = true;

            this.Name = apiOperationType.Name.Replace("Query", string.Empty).Replace("Command", string.Empty).ToPascalCase();
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
        /// The source of this operation descriptor. Useful for determining _how_ an operation
        /// has found (i.e. scan vs explicit).
        /// </summary>
        public string Source { get; }

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
        public bool IsCommand => typeof(ICommand).IsAssignableFrom(this.OperationType);

        /// <summary>
        /// Gets all registered <see cref="ApiOperationLink" />s for this descriptor.
        /// </summary>
        public IReadOnlyList<ApiOperationLink> Links => this._links;

        /// <summary>
        /// Gets the list of response types this operation could generate, which should typically only
        /// be a single Type to avoid clients having to deal with multiple possible responses
        /// </summary>
        public IReadOnlyList<ResponseDescriptor> Responses => this._responses;

        /// <summary>
        /// The handlers that have been registered for this descriptor.
        /// </summary>
        public IReadOnlyList<IOperationExecutorBuilder> Handlers => this._handlers;

        /// <summary>
        /// Indicates whether this operation allows for multiple handlers. This may be turned off if only a single handler
        /// makes sense for a given operation, for example in the context of a HTTP service.
        /// </summary>
        public bool AllowMultipleHandlers { get; set; }

        /// <summary>
        /// Indicates whether this operation requires a return value. This may be turned off if the host does not require a
        /// result, for example for message publishing, where it does not matter if 0 handlers or multiple execute.
        /// </summary>
        public bool RequiresReturnValue { get; set; }

        /// <summary>
        /// Registers the given handler with this operation, identifying what could possible handle and execute
        /// this operation.
        /// </summary>
        /// <remarks>
        /// Multiple handlers <b>MAY</b> be registered against a single <see cref="ApiOperationDescriptor" />, although
        /// this behaviour can be stopped by setting <see cref="AllowMultipleHandlers" /> to <c>false</c>.
        /// </remarks>
        /// <param name="builder">The handler to register.</param>
        public void RegisterHandler(IOperationExecutorBuilder builder)
        {
            if (this.AllowMultipleHandlers == false && this._handlers.Count > 0)
            {
                throw new InvalidOperationException($@"Cannot add multiple handlers to the operation {this}.

If multiple handlers should be enabled, and the host accepts that, {nameof(this.AllowMultipleHandlers)} can be set to true. If not then ensure you are not registering multiple handlers that handle this operation (or any interface it implements or class it inherits).

The handlers found are:

{this._handlers.Single()}
{builder}");
            }

            this._handlers.Add(builder);
        }

        /// <summary>
        /// Adds a link for this descriptor.
        /// </summary>
        /// <param name="apiOperationLink">The link to add.</param>
        /// <exception cref="NotImplementedException">If the link has not been created for this descriptor.</exception>
        public void AddLink(ApiOperationLink apiOperationLink)
        {
            if (apiOperationLink.OperationDescriptor != this)
            {
                throw new InvalidOperationException($"The ApiOperationLink MUST have been created for this ApiOperationDescriptor, but was instead created for the description {apiOperationLink.OperationDescriptor}");
            }

            this._links.Add(apiOperationLink);
        }

        /// <summary>
        /// Adds a <see cref="ResponseDescriptor" /> to this descriptor, describing one possible response that
        /// can be generated by this operation.
        /// </summary>
        /// <param name="responseDescriptor">The response descriptor.</param>
        public void AddResponse(ResponseDescriptor responseDescriptor)
        {
            Guard.NotNull(nameof(responseDescriptor), responseDescriptor);

            this._responses.Add(responseDescriptor);
        }

        /// <summary>
        /// Gets the feature data of the specified type, which provides a pluggable mechanism to add additional
        /// structured data to <see cref="ApiOperationDescriptor" />s, such as HTTP-related data.
        /// </summary>
        /// <typeparam name="T">The feature data type to load.</typeparam>
        /// <returns>The feature data of the specified type.</returns>
        /// <exception cref="InvalidOperationException">If no such feature data exists.</exception>
        public T GetFeatureData<T>()
        {
            if (this.TryGetFeatureData<T>(out var featureData) == false)
            {
                throw new InvalidOperationException($"Could not find feature data of type {typeof(T).FullName}");
            }

            return featureData;
        }

        /// <summary>
        /// Tries to get the feature data of the specified type, which provides a pluggable mechanism to add additional
        /// structured data to <see cref="ApiOperationDescriptor" />s, such as HTTP-related data.
        /// </summary>
        /// <param name="feature">The feature data of the specified type.</param>
        /// <typeparam name="T">The feature data type to load.</typeparam>
        /// <returns>Whether the feature data exists.</returns>
        public bool TryGetFeatureData<T>(out T feature)
        {
            var key = typeof(T).FullName;

            if (this._featureData.TryGetValue(key, out var data))
            {
                feature = (T)data;

                return true;
            }

            feature = default;
            return false;
        }

        /// <summary>
        /// Adds the given feature data to this descriptor.
        /// </summary>
        /// <param name="newFeatureData">The feature data to store.</param>
        public void SetFeatureData(object newFeatureData)
        {
            Guard.NotNull(nameof(this._featureData), this._featureData);

            var key = newFeatureData.GetType().FullName;

            this._featureData[key] = newFeatureData;
        }

        /// <summary>
        /// Creates a new instance of the API operation this descriptor describes, with the default implementation
        /// to simply be to use <see cref="Activator"/>, which means operations must have a public, parameterless
        /// constructor.
        /// </summary>
        /// <returns>A new instance of operation this descriptor describes.</returns>
        public object CreateInstance()
        {
            return Activator.CreateInstance(this.OperationType);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.OperationType.FullName;
        }
    }
}
