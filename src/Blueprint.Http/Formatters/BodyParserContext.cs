using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Http.Formatters;

/// <summary>
/// A context object used by an input formatter for deserializing the request body into an object.
/// </summary>
public class BodyParserContext
{
    /// <summary>
    /// Creates a new instance of <see cref="BodyParserContext"/>.
    /// </summary>
    /// <param name="operationContext">The operation context.</param>
    /// <param name="httpContext">
    /// The <see cref="Microsoft.AspNetCore.Http.HttpContext"/> for the current operation.
    /// </param>
    /// <param name="instance">The instance that needs to be populated.</param>
    /// <param name="bodyType">The type the body should be read as.</param>
    public BodyParserContext(
        ApiOperationContext operationContext,
        HttpContext httpContext,
        object instance,
        Type bodyType)
    {
        this.OperationContext = operationContext ?? throw new ArgumentNullException(nameof(httpContext));
        this.HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        this.BodyType = bodyType ?? throw new ArgumentNullException(nameof(bodyType));
    }

    /// <summary>
    /// The operation context.
    /// </summary>
    public ApiOperationContext OperationContext { get; }

    /// <summary>
    /// Gets the <see cref="Microsoft.AspNetCore.Http.HttpContext"/> associated with the current operation.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// Gets the requested <see cref="Type"/> of the request body deserialization.
    /// </summary>
    public Type BodyType { get; }

    /// <summary>
    /// The model instance to be populated.
    /// </summary>
    public object Instance { get; }
}