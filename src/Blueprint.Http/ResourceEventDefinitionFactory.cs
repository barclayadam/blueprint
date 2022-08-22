using System;
using JetBrains.Annotations;

namespace Blueprint.Http;

/// <summary>
/// A base class for creating factories
/// </summary>
/// <typeparam name="TResource"></typeparam>
/// <typeparam name="TReturn"></typeparam>
public abstract class ResourceEventDefinitionFactoryBase<TResource, TReturn> where TResource : ApiResource
{
    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" /> with a change type
    /// of <see cref="ResourceEventChangeType.Created" /> and an event sub id of <c>created</c>.
    /// </summary>
    /// <returns>A new resource event definition.</returns>
    public TReturn Created()
    {
        return this.Create(
            ResourceEventChangeType.Created,
            ResourceEvent<TResource>.CreateId("created"));
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" /> with a change type
    /// of <see cref="ResourceEventChangeType.Created" /> and the given event sub id.
    /// </summary>
    /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
    /// <returns>A new resource event definition.</returns>
    public TReturn Created(string eventSubId)
    {
        return this.Create(
            ResourceEventChangeType.Created,
            ResourceEvent<TResource>.CreateId(eventSubId));
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" /> with a change type
    /// of <see cref="ResourceEventChangeType.Updated" /> and an event sub id of <c>updated</c>.
    /// </summary>
    /// <returns>A new resource event definition.</returns>
    public TReturn Updated()
    {
        return this.Create(
            ResourceEventChangeType.Updated,
            ResourceEvent<TResource>.CreateId("updated"));
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" /> with a change type
    /// of <see cref="ResourceEventChangeType.Updated" /> and the given event sub id.
    /// </summary>
    /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
    /// <returns>A new resource event definition.</returns>
    public TReturn Updated(string eventSubId)
    {
        return this.Create(
            ResourceEventChangeType.Updated,
            ResourceEvent<TResource>.CreateId(eventSubId));
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" /> with a change type
    /// of <see cref="ResourceEventChangeType.Deleted" /> and an event sub id of <c>deleted</c>.
    /// </summary>
    /// <returns>A new resource event definition.</returns>
    public TReturn Deleted()
    {
        return this.Create(
            ResourceEventChangeType.Deleted,
            ResourceEvent<TResource>.CreateId("deleted"));
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" /> with a change type
    /// of <see cref="ResourceEventChangeType.Deleted" /> and the given event sub id.
    /// </summary>
    /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
    /// <returns>A new resource event definition.</returns>
    public TReturn Deleted(string eventSubId)
    {
        return this.Create(
            ResourceEventChangeType.Deleted,
            ResourceEvent<TResource>.CreateId(eventSubId));
    }

    /// <summary>
    /// Creates the actual mapper from this factory.
    /// </summary>
    /// <param name="type">The change type.</param>
    /// <param name="id">The Id.</param>
    /// <returns>A mapper for the given arguments.</returns>
    protected abstract TReturn Create(ResourceEventChangeType type, string id);
}

/// <summary>
/// A factory to create instances of <see cref="ResourceEventDefinition{TResource}" /> when no mapping exists and we expect
/// every use of directly pass the <see cref="ApiResource" />.
/// </summary>
/// <example>
/// public static class TenantEvents {
///     private static readonly ResourceEventFactoryFactory&lt;TenantApiResource&gt; Factory =
///         ResourceEventFactory.For&lt;TenantApiResource&gt;();
///
///     public static readonly ResourceEventFactory&lt;TenantApiResource&gt; Created = Factory.Created();
///     public static readonly ResourceEventFactory&lt;TenantApiResource&gt; Updated = Factory.Updated("modified");
///     public static readonly ResourceEventFactory&lt;TenantApiResource&gt; SignedUp = Factory.Updated("signedUp");
/// }
/// </example>
/// <typeparam name="TResource">The type of resource that has been modified.</typeparam>
public class ResourceEventDefinitionFactory<TResource> : ResourceEventDefinitionFactoryBase<TResource, ResourceEventDefinition<TResource>> where TResource : ApiResource
{
    /// <inheritdoc/>
    protected override ResourceEventDefinition<TResource> Create(ResourceEventChangeType type, string id)
    {
        return new ResourceEventDefinition<TResource>(type, id);
    }
}

/// <summary>
/// A factory to create instances of <see cref="ResourceEventDefinitionWithMapper{TResource}" /> without having to
/// specify a mapper for every instance, as that is inherited.
/// </summary>
/// <example>
/// public static class TenantEvents {
///     private static readonly ResourceEventFactoryFactory&lt;TenantApiResource&gt; Factory =
///         ResourceEventFactory.For(() => new GetCurrentTenantQuery());
///
///     public static readonly ResourceEventFactory&lt;TenantApiResource&gt; Created = Factory.Created();
///     public static readonly ResourceEventFactory&lt;TenantApiResource&gt; Updated = Factory.Updated("modified");
///     public static readonly ResourceEventFactory&lt;TenantApiResource&gt; SignedUp = Factory.Updated("signedUp");
/// }
/// </example>
/// <typeparam name="TResource">The type of resource that has been modified.</typeparam>
public class ResourceEventDefinitionFactoryWithMapper<TResource>
    : ResourceEventDefinitionFactoryBase<TResource, ResourceEventDefinitionWithMapper<TResource>> where TResource : ApiResource
{
    [CanBeNull]
    private readonly Func<TResource> _mapper;

    [CanBeNull]
    private readonly Func<IQuery<TResource>> _queryMapper;

    /// <summary>
    /// Initialises a new instance of the <see cref="ResourceEventDefinitionFactoryWithMapper{TResource}" /> class.
    /// </summary>
    /// <param name="queryMapper">A mapper to construct a "self" API operation.</param>
    public ResourceEventDefinitionFactoryWithMapper(Func<IQuery<TResource>> queryMapper)
    {
        this._queryMapper = queryMapper;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ResourceEventDefinitionFactoryWithMapper{TResource}" /> class.
    /// </summary>
    /// <param name="mapper">A mapper to construct the final resource.</param>
    public ResourceEventDefinitionFactoryWithMapper(Func<TResource> mapper)
    {
        this._mapper = mapper;
    }

    /// <inheritdoc/>
    protected override ResourceEventDefinitionWithMapper<TResource> Create(ResourceEventChangeType type, string id)
    {
        return this._mapper != null ?
            new ResourceEventDefinitionWithMapper<TResource>(
                type,
                id,
                this._mapper) :

            new ResourceEventDefinitionWithMapper<TResource>(
                type,
                id,
                this._queryMapper);
    }
}

/// <summary>
/// A factory to create instances of <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" /> without having to
/// specify a mapper for every instance, as that is inherited.
/// </summary>
/// <example>
/// public static class UserEvents {
///     private static readonly ResourceEventFactoryFactory&lt;UserApiResource, User&gt; Factory =
///         ResourceEventFactory.For((User u) => new GetUserQuery { Id = u.Id });
///
///     public static readonly ResourceEventFactory&lt;UserApiResource, User&gt; Created = Factory.Created();
///     public static readonly ResourceEventFactory&lt;UserApiResource, User&gt; Updated = Factory.Updated("modified");
///     public static readonly ResourceEventFactory&lt;UserApiResource, User&gt; Enabled = Factory.Updated("enabled");
///     public static readonly ResourceEventFactory&lt;UserApiResource, User&gt; Disabled = Factory.Updated("disabled");
///     public static readonly ResourceEventFactory&lt;UserApiResource, User&gt; Disabled = Factory.Updated("addedToGroup");
///     public static readonly ResourceEventFactory&lt;UserApiResource, User&gt; Deleted = Factory.Deleted();
/// }
/// </example>
/// <typeparam name="TResource">The type of resource that has been modified.</typeparam>
/// <typeparam name="TDomain">A "domain" type that will be passed in to a mapper when creating an instance from
/// this definition.</typeparam>
public class ResourceEventDefinitionFactoryWithMapper<TResource, TDomain>
    : ResourceEventDefinitionFactoryBase<TResource, ResourceEventDefinitionWithMapper<TResource, TDomain>> where TResource : ApiResource
{
    [CanBeNull]
    private readonly Func<TDomain, TResource> _mapper;

    [CanBeNull]
    private readonly Func<TDomain, IQuery<TResource>> _queryMapper;

    /// <summary>
    /// Initialises a new instance of the <see cref="ResourceEventDefinitionFactoryWithMapper{TResource,TDomain}" /> class.
    /// </summary>
    /// <param name="queryMapper">A mapper to construct a "self" API operation.</param>
    public ResourceEventDefinitionFactoryWithMapper(Func<TDomain, IQuery<TResource>> queryMapper)
    {
        this._queryMapper = queryMapper;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ResourceEventDefinitionFactoryWithMapper{TResource,TDomain}" /> class.
    /// </summary>
    /// <param name="mapper">A mapper to construct the final resource.</param>
    public ResourceEventDefinitionFactoryWithMapper(Func<TDomain, TResource> mapper)
    {
        this._mapper = mapper;
    }

    /// <inheritdoc/>
    protected override ResourceEventDefinitionWithMapper<TResource, TDomain> Create(ResourceEventChangeType type, string id)
    {
        return this._mapper != null ?
            new ResourceEventDefinitionWithMapper<TResource, TDomain>(
                type,
                id,
                this._mapper) :

            new ResourceEventDefinitionWithMapper<TResource, TDomain>(
                type,
                id,
                this._queryMapper);
    }
}
