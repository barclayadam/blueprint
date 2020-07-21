using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Blueprint.Apm;
using Blueprint.Authorisation;

namespace Blueprint
{
    /// <summary>
    /// The context of an API operation, the object that is passed to all middleware and final operation handlers
    /// to allow sharing of state, for example the operation being executed, or an authentication context that
    /// gets created.
    /// </summary>
    public class ApiOperationContext
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ApiOperationContext" /> class, using
        /// <see cref="ApiOperationDescriptor.CreateInstance" /> to construct the operation
        /// instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider (typically a nested scope) for this context.</param>
        /// <param name="dataModel">The data model that represents the API in which this context is being executed.</param>
        /// <param name="operationDescriptor">A descriptor for the operation that is being executed.</param>
        /// <param name="token">A cancellation token to indicate the operation should stop.</param>
        public ApiOperationContext(
            IServiceProvider serviceProvider,
            ApiDataModel dataModel,
            ApiOperationDescriptor operationDescriptor,
            CancellationToken token)
        {
            Guard.NotNull(nameof(serviceProvider), serviceProvider);
            Guard.NotNull(nameof(dataModel), dataModel);
            Guard.NotNull(nameof(operationDescriptor), operationDescriptor);

            Descriptor = operationDescriptor;
            OperationCancelled = token;
            DataModel = dataModel;
            ServiceProvider = serviceProvider;
            Operation = operationDescriptor.CreateInstance();

            Data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ApiOperationContext" /> class using the already
        /// created operation instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider (typically a nested scope) for this context.</param>
        /// <param name="dataModel">The data model that represents the API in which this context is being executed.</param>
        /// <param name="operation">The operation instance.</param>
        /// <param name="token">A cancellation token to indicate the operation should stop.</param>
        public ApiOperationContext(
            IServiceProvider serviceProvider,
            ApiDataModel dataModel,
            object operation,
            CancellationToken token)
        {
            Guard.NotNull(nameof(serviceProvider), serviceProvider);
            Guard.NotNull(nameof(dataModel), dataModel);
            Guard.NotNull(nameof(operation), operation);

            var operationType = operation.GetType();

            Descriptor = dataModel.Operations.SingleOrDefault(d => d.OperationType == operationType);

            if (Descriptor == null)
            {
                throw new ArgumentException(
                    $"Could not find descriptor for operation of type {operationType}",
                    nameof(operation));
            }

            OperationCancelled = token;
            DataModel = dataModel;
            ServiceProvider = serviceProvider;
            Operation = operation;

            Data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the data model for this operation.
        /// </summary>
        public ApiDataModel DataModel { get; }

        /// <summary>
        /// Gets the operation descriptor that described the operation that is currently being executed.
        /// </summary>
        public ApiOperationDescriptor Descriptor { get; }

        /// <summary>
        /// Gets the cancellation token that is used to indicate / request cancellation of an operation.
        /// </summary>
        public CancellationToken OperationCancelled { get; }

        /// <summary>
        /// Gets the operation that is currently being executed.
        /// </summary>
        public object Operation { get; }

        /// <summary>
        /// The <see cref="IServiceProvider" /> associated with this operation execution, allowing middleware and
        /// handlers to get runtime dependencies out of the correctly scoped container.
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Gets the parent <see cref="ApiOperationContext" /> of this context, which may be null.
        /// </summary>
        public ApiOperationContext Parent { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IApmSpan" /> for this context, which may be <c>null</c> if the host
        /// of an operation has not integrated with APM tooling.
        /// </summary>
        public IApmSpan ApmSpan { get; set; }

        /// <summary>
        /// Gets a value indicating whether this is a nested context, meaning it has been created as a child of an
        /// existing context, used to execute another operation in the context of an existing one.
        /// </summary>
        public bool IsNested => Parent != null;

        /// <summary>
        /// Gets or sets a value indicating whether to skip authorisation when executing the operation of this context,
        /// which should be used with extreme care.
        /// </summary>
        /// <remarks>
        /// This is typically used when running a child operation where the authorisation of the parent is enough to indicate
        /// the child can be successfully executed.
        /// </remarks>
        public bool SkipAuthorisation { get; set; }

        public IUserAuthorisationContext UserAuthorisationContext { get; set; }

        public ClaimsIdentity ClaimsIdentity { get; set; }

        /// <summary>
        /// Provides a generic means of middleware components or hosts to store data related to an API operation
        /// to be used throughout the processing pipeline.
        /// </summary>
        public Dictionary<string, object> Data { get; private set; }

        public ApiOperationContext CreateNested(Type type)
        {
            Guard.NotNull(nameof(type), type);

            var context = DataModel.CreateOperationContext(ServiceProvider, type, OperationCancelled);

            PopulateNested(context);

            return context;
        }

        public ApiOperationContext CreateNested(object operation)
        {
            Guard.NotNull(nameof(operation), operation);

            var context = DataModel.CreateOperationContext(ServiceProvider, operation, OperationCancelled);

            PopulateNested(context);

            return context;
        }

        private void PopulateNested(ApiOperationContext childContext)
        {
            childContext.Parent = this;
            childContext.Data = Data;
            childContext.ClaimsIdentity = ClaimsIdentity;
            childContext.UserAuthorisationContext = UserAuthorisationContext;
        }
    }
}
