﻿using System;
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
        {
            Guard.NotNull(nameof(serviceProvider), serviceProvider);
            Guard.NotNull(nameof(dataModel), dataModel);
            Guard.NotNull(nameof(operationDescriptor), operationDescriptor);

            DataModel = dataModel;
            Descriptor = operationDescriptor;

            ServiceProvider = serviceProvider;

            Operation = operationDescriptor.CreateInstance();

            Data = new Dictionary<string, object>();
        }

        public ApiOperationContext(
            IServiceProvider serviceProvider,
            ApiDataModel dataModel,
            ApiOperationDescriptor operationDescriptor,
            IApiOperation instance) :
            this(serviceProvider, dataModel, operationDescriptor)
        {
            Guard.NotNull(nameof(instance), instance);

            if (!operationDescriptor.OperationType.IsInstanceOfType(instance))
            {
                throw new InvalidOperationException($"Instance of type {instance.GetType().Name} is not compatible with descriptor {operationDescriptor}");
            }

            Operation = instance;
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

        /// <summary>
        /// Gets the <see cref="IServiceProvider" /> associated with this operation execution, allowing middleware and
        /// handlers to get runtime dependencies out of the correctly scoped container.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

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

            var context = DataModel.CreateOperationContext(ServiceProvider, type);

            context.Data = Data;
            context.HttpContext = HttpContext;

            return context;
        }
    }
}