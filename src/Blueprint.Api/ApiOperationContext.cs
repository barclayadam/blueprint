using System;
using System.Collections.Generic;
using System.Security.Claims;
using Blueprint.Core;
using Blueprint.Core.Security;
using Microsoft.AspNetCore.Http;
using StructureMap;

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
            IContainer container,
            ApiDataModel dataModel,
            ApiOperationDescriptor operationDescriptor)
        {
            Container = container;
            DataModel = dataModel;
            Descriptor = operationDescriptor;

            Operation = operationDescriptor.CreateInstance();

            Data = new Dictionary<string, object>();
        }

        public ApiOperationContext(
            IContainer container,
            ApiDataModel dataModel,
            ApiOperationDescriptor operationDescriptor,
            IApiOperation instance)
        {
            Guard.NotNull(nameof(instance), instance);

            if (!operationDescriptor.OperationType.IsInstanceOfType(instance))
            {
                throw new InvalidOperationException($"Instance of type {instance.GetType().Name} is not compatible with descriptor {operationDescriptor}");
            }

            Container = container;
            DataModel = dataModel;
            Descriptor = operationDescriptor;

            Operation = instance;

            Data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the data model for this operation.
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

        public IContainer Container { get; }

        public IUserAuthorisationContext UserAuthorisationContext { get; set; }

        public ClaimsIdentity ClaimsIdentity { get; set; }

        public Dictionary<string, object> Data { get; private set; }

        public HttpContext HttpContext { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpRequest" /> that initiated this API call.
        /// </summary>
        public HttpRequest Request => HttpContext?.Request;

        public IDictionary<string, object> RouteData { get; set; }

        /// <summary>
        /// Gets the <see cref="HttpResponse" /> that will eventually be written back to the client, the result
        /// of this API request.
        /// </summary>
        public HttpResponse Response => HttpContext?.Response;

        public Exception Exception { get; set; }

        public ApiOperationContext CreateChild(Type type)
        {
            Guard.NotNull(nameof(type), type);

            var context = DataModel.CreateOperationContext(Container, type);

            context.Data = Data;
            context.HttpContext = HttpContext;

            return context;
        }
    }
}
