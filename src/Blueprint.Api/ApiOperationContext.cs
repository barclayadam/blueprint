using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Blueprint.Core;
using Blueprint.Core.Authorisation;

namespace Blueprint.Api
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
        public ApiOperationContext(
            IServiceProvider serviceProvider,
            ApiDataModel dataModel,
            ApiOperationDescriptor operationDescriptor)
        {
            Guard.NotNull(nameof(serviceProvider), serviceProvider);
            Guard.NotNull(nameof(dataModel), dataModel);
            Guard.NotNull(nameof(operationDescriptor), operationDescriptor);

            Descriptor = operationDescriptor;
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
        /// <param name="instance">The operation instance.</param>
        public ApiOperationContext(
            IServiceProvider serviceProvider,
            ApiDataModel dataModel,
            IApiOperation instance)
        {
            Guard.NotNull(nameof(serviceProvider), serviceProvider);
            Guard.NotNull(nameof(dataModel), dataModel);
            Guard.NotNull(nameof(instance), instance);

            var operationType = instance.GetType();

            Descriptor = dataModel.Operations.SingleOrDefault(d => d.OperationType == operationType);

            if (Descriptor == null)
            {
                throw new ArgumentException(
                    $"Could not find descriptor for operation of type {operationType}",
                    nameof(instance));
            }

            DataModel = dataModel;
            ServiceProvider = serviceProvider;
            Operation = instance;

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
        /// Gets the operation that is currently being executed.
        /// </summary>
        public IApiOperation Operation { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider" /> associated with this operation execution, allowing middleware and
        /// handlers to get runtime dependencies out of the correctly scoped container.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the parent <see cref="ApiOperationContext" /> of this context, which may be null.
        /// </summary>
        public ApiOperationContext Parent { get; set; }

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

            var context = DataModel.CreateOperationContext(ServiceProvider, type);

            PopulateChild(context);

            return context;
        }

        public ApiOperationContext CreateNested(IApiOperation operation)
        {
            Guard.NotNull(nameof(operation), operation);

            var context = DataModel.CreateOperationContext(ServiceProvider, operation);

            PopulateChild(context);

            return context;
        }

        private void PopulateChild(ApiOperationContext childContext)
        {
            childContext.Parent = this;
            childContext.Data = Data;
            childContext.ClaimsIdentity = ClaimsIdentity;
            childContext.UserAuthorisationContext = UserAuthorisationContext;
        }
    }
}
