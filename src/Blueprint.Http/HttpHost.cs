using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Blueprint.Configuration;
using Blueprint.Middleware;

namespace Blueprint.Http;

/// <summary>
/// An <see cref="IOperationScannerConvention" /> that adds HTTP-related feature
/// details to <see cref="ApiOperationDescriptor" />s and excludes any operations that
/// have no <see cref="LinkAttribute" />.
/// </summary>
public class HttpHost : IOperationScannerConvention, IBlueprintHost
{
    private static readonly IReadOnlyList<ApiOperationLink> _emptyLinkList = Array.Empty<ApiOperationLink>();

    private readonly List<ApiOperationLink> _allLinks = new List<ApiOperationLink>();

    // Dictionary lookups, contains values from allLinks
    private readonly Dictionary<Type, List<ApiOperationLink>> _operationTypeToLinks = new Dictionary<Type, List<ApiOperationLink>>();
    private readonly Dictionary<Type, List<ApiOperationLink>> _resourceTypeToLinks = new Dictionary<Type, List<ApiOperationLink>>();

    /// <summary>
    /// Gets all registered links for this model, with a link representing the association between a resource and a different
    /// operation (for example a link between the resource 'Jobs' and the operation 'Apply').
    /// </summary>
    public IEnumerable<ApiOperationLink> Links => this._allLinks;

    /// <inheritdoc />
    public void Apply(ApiOperationDescriptor descriptor)
    {
        if (!this.IsSupported(descriptor.OperationType))
        {
            return;
        }

        var httpMethodAttribute = descriptor.OperationType.GetCustomAttribute<HttpMethodAttribute>(true);

        // By default, command are POST and everything else GET
        var supportedMethod = httpMethodAttribute != null ? httpMethodAttribute.HttpMethod :
            typeof(ICommand).IsAssignableFrom(descriptor.OperationType) ? "POST" : "GET";

        var httpOperationFeatureData = new HttpOperationFeatureData(this, supportedMethod);

        // TODO: Add default route
        foreach (var linkAttribute in descriptor.OperationType.GetCustomAttributes<LinkAttribute>())
        {
            var apiOperationLink = new ApiOperationLink(descriptor, linkAttribute.RoutePattern, linkAttribute.Rel ?? descriptor.Name, linkAttribute.ResourceType);

            httpOperationFeatureData.AddLink(apiOperationLink);

            this.RegisterLink(apiOperationLink);
        }

        descriptor.SetFeatureData(httpOperationFeatureData);

        descriptor.AllowMultipleHandlers = false;
        descriptor.RequiresReturnValue = true;

        RegisterResponses(descriptor);
    }

    /// <inheritdoc />
    public bool IsSupported(Type operationType)
    {
        return operationType.GetCustomAttributes<LinkAttribute>().Any();
    }

    /// <summary>
    /// Registers the specified link, checking that the link represents a unique combination of HTTP method
    /// and URL format to avoid clashes.
    /// </summary>
    /// <remarks>
    /// In addition the link <see cref="ApiOperationLink.Rel" /> property will be checked to ensure no two links with
    /// the same rel can be associated with a single resource type.
    /// </remarks>
    /// <param name="link">The link to register.</param>
    /// <exception cref="InvalidOperationException">If the link is not unique.</exception>
    private void RegisterLink(ApiOperationLink link)
    {
        var existing = this._allLinks.Where(l =>
                l.UrlFormat.Equals(link.UrlFormat, StringComparison.CurrentCultureIgnoreCase) &&
                l.OperationDescriptor.Name == link.OperationDescriptor.Name &&
                l.ResourceType == link.ResourceType)
            .ToList();

        if (existing.Any())
        {
            throw new InvalidOperationException(
                $"Could not register {link} from {link.OperationDescriptor.Source} as it conflicts with existing Link registrations:\n\n" +
                string.Join("\n", existing.Select(e => $" {e} sourced from {e.OperationDescriptor.Source}")));
        }

        this._allLinks.Add(link);

        if (!link.IsRootLink)
        {
            if (!this._resourceTypeToLinks.ContainsKey(link.ResourceType))
            {
                this._resourceTypeToLinks[link.ResourceType] = new List<ApiOperationLink>();
            }

            var existingLink = this._resourceTypeToLinks[link.ResourceType].SingleOrDefault(l => l.Rel == link.Rel);

            if (existingLink != null)
            {
                throw new InvalidOperationException(
                    $"Duplicate link found for resource type {link.ResourceType.Name} with rel {link.Rel}. Existing link is for the operation: \n\n" +
                    $" {existingLink.OperationDescriptor.OperationType.FullName}: {existingLink.UrlFormat}\n\n" +
                    "Every link must be a unique pairing of resource type (i.e. UserApiResource) and rel (i.e. \"self\" or \"activate\")");
            }

            this._resourceTypeToLinks[link.ResourceType].Add(link);
        }

        if (!this._operationTypeToLinks.ContainsKey(link.OperationDescriptor.OperationType))
        {
            this._operationTypeToLinks[link.OperationDescriptor.OperationType] = new List<ApiOperationLink>();
        }

        this._operationTypeToLinks[link.OperationDescriptor.OperationType].Add(link);
    }

    /// <summary>
    /// Gets all registered links for the given resource type. Links can be associated with a resource so that they get
    /// generated and returned in the $links properties of an <see cref="ILinkableResource"/> instance.
    /// </summary>
    /// <param name="resourceType">The resource type to load links for.</param>
    /// <returns>The (potentially empty) enumeration of registered links.</returns>
    public IReadOnlyList<ApiOperationLink> GetLinksForResource(Type resourceType)
    {
        Guard.NotNull(nameof(resourceType), resourceType);

        if (!this._resourceTypeToLinks.ContainsKey(resourceType))
        {
            return _emptyLinkList;
        }

        return this._resourceTypeToLinks[resourceType];
    }

    /// <summary>
    /// Gets the link that has been registered for the given resource type with the specified 'rel'
    /// property.
    /// </summary>
    /// <param name="resourceType">The resource type to load for.</param>
    /// <param name="rel">The rel of the link to load.</param>
    /// <returns>The associated link, or <c>null</c> if no such link exists.</returns>
    public ApiOperationLink GetLinkFor(Type resourceType, string rel)
    {
        Guard.NotNull(nameof(resourceType), resourceType);

        if (!this._resourceTypeToLinks.TryGetValue(resourceType, out var links))
        {
            return null;
        }

        foreach (var link in links)
        {
            if (link.Rel == rel)
            {
                return link;
            }
        }

        return null;
    }

    private static void RegisterResponses(ApiOperationDescriptor descriptor)
    {
        var typedOperation = descriptor.OperationType
            .GetInterfaces()
            .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReturn<>));

        if (typedOperation != null)
        {
            var returnType = typedOperation.GetGenericArguments()[0];

            // If we have a StatusCodeResult then we either
            // 1. Have a specific subclass and therefore know the expected response code and can therefore add a response
            // 2. Have the base class and therefore can not determine the actual expected response code so leave it open and do not add anything specific
            if (typeof(StatusCodeResult).IsAssignableFrom(returnType))
            {
                var instanceProperty = returnType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);

                if (instanceProperty != null)
                {
                    // This is option 1, we have a specific subclass (see the .tt file that generates these, i.e. StatusCodeResult.Created)
                    var statusCode = ((StatusCodeResult)instanceProperty.GetValue(null)).StatusCode;

                    descriptor.AddResponse(
                        new ResponseDescriptor((int)statusCode, statusCode.ToString()));
                }
            }
            else
            {
                descriptor.AddResponse(
                    new ResponseDescriptor(returnType, (int)HttpStatusCode.OK, HttpStatusCode.OK.ToString()));
            }
        }

        descriptor.AddResponse(
            new ResponseDescriptor(typeof(UnhandledExceptionOperationResult), 500, "Unexpected error"));

        descriptor.AddResponse(
            new ResponseDescriptor(typeof(ValidationFailedOperationResult), 422, "Validation failure"));
    }
}
