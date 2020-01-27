using System;
using System.Collections.Generic;
using System.Security.Claims;
using Blueprint.Core;
using Blueprint.Core.Authorisation;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Api
{
    /// <summary>
    /// The context of an API operation, the object that is passed to all middleware and final operation handlers
    /// to allow sharing of state, for example the operation being executed, or an authentication context that
    /// gets created.
    /// </summary>
    public class ApiOperationContext
    {
        public ApiOperationContext(
            IServiceProvider serviceProvider,
            ApiDataModel dataModel,
            ApiOperationDescriptor operationDescriptor)
            : this(serviceProvider, dataModel, operationDescriptor, operationDescriptor.CreateInstance())
        {
        }

        public ApiOperationContext(
            IServiceProvider serviceProvider,
            ApiDataModel dataModel,
            ApiOperationDescriptor operationDescriptor,
            IApiOperation instance)
        {
            Guard.NotNull(nameof(serviceProvider), serviceProvider);
            Guard.NotNull(nameof(dataModel), dataModel);
            Guard.NotNull(nameof(operationDescriptor), operationDescriptor);
            Guard.NotNull(nameof(instance), instance);

            if (!operationDescriptor.OperationType.IsInstanceOfType(instance))
            {
                throw new InvalidOperationException($"Instance of type {instance.GetType().Name} is not compatible with descriptor {operationDescriptor}");
            }

            DataModel = dataModel;
            Descriptor = operationDescriptor;

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

        public Dictionary<string, object> Data { get; private set; }

        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// Gets the <see cref="HttpRequest" /> that initiated this API call.
        /// </summary>
        public HttpRequest Request => HttpContext?.Request;

        /// <summary>
        /// Gets the <see cref="HttpResponse" /> that will eventually be written back to the client, the result
        /// of this API request.
        /// </summary>
        public HttpResponse Response => HttpContext?.Response;

        public IDictionary<string, object> RouteData { get; set; }

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
            childContext.HttpContext = HttpContext;
            childContext.ClaimsIdentity = ClaimsIdentity;
            childContext.UserAuthorisationContext = UserAuthorisationContext;
            childContext.RouteData = RouteData;
        }
    }
}
