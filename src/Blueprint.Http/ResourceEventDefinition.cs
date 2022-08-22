using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blueprint.Http;

/// <summary>
/// An entry point to constructing <see cref="ResourceEventDefinition{TResource}" /> instances.
/// </summary>
public static class ResourceEventDefinition
{
    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionFactoryWithMapper{TResource, TDomain}" /> with the specified mapper, to be
    /// used by client applications to then construct the individual <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" />
    /// instances without specifying the mapper multiple times.
    /// </summary>
    /// <param name="mapper">The mapper used to construct self query operation instances.</param>
    /// <typeparam name="TResource">The resource type contained within the created resource events.</typeparam>
    /// <typeparam name="TDomain">A "domain" type that will be passed in to a mapper when creating an instance from
    /// this definition.</typeparam>
    /// <returns>A new factory.</returns>
    public static ResourceEventDefinitionFactoryWithMapper<TResource, TDomain> For<TResource, TDomain>(Func<TDomain, IQuery<TResource>> mapper) where TResource : ApiResource
    {
        return new ResourceEventDefinitionFactoryWithMapper<TResource, TDomain>(mapper);
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionFactoryWithMapper{TResource, TDomain}" /> with the specified mapper, to be
    /// used by client applications to then construct the individual <see cref="ResourceEventDefinitionFactoryWithMapper{TResource, TDomain}" />
    /// instances without specifying the mapper multiple times.
    /// </summary>
    /// <param name="mapper">The mapper used to construct self query operation instances.</param>
    /// <typeparam name="TResource">The resource type contained within the created resource events.</typeparam>
    /// <typeparam name="TDomain">A "domain" type that will be passed in to a mapper when creating an instance from
    /// this definition.</typeparam>
    /// <returns>A new factory.</returns>
    public static ResourceEventDefinitionFactoryWithMapper<TResource, TDomain> For<TResource, TDomain>(Func<TDomain, TResource> mapper) where TResource : ApiResource
    {
        return new ResourceEventDefinitionFactoryWithMapper<TResource, TDomain>(mapper);
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionFactoryWithMapper{TResource}" /> with the specified mapper, to be
    /// used by client applications to then construct the individual <see cref="ResourceEventDefinitionFactoryWithMapper{TResource}" />
    /// instances without specifying the mapper multiple times.
    /// </summary>
    /// <param name="mapper">The mapper used to construct self query operation instances.</param>
    /// <typeparam name="TResource">The resource type contained within the created resource events.</typeparam>
    /// <returns>A new factory.</returns>
    public static ResourceEventDefinitionFactoryWithMapper<TResource> For<TResource>(Func<IQuery<TResource>> mapper) where TResource : ApiResource
    {
        return new ResourceEventDefinitionFactoryWithMapper<TResource>(mapper);
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionFactoryWithMapper{TResource}" /> with the specified mapper, to be
    /// used by client applications to then construct the individual <see cref="ResourceEventDefinitionFactoryWithMapper{TResource}" />
    /// instances without specifying the mapper multiple times.
    /// </summary>
    /// <param name="mapper">The mapper used to construct self query operation instances.</param>
    /// <typeparam name="TResource">The resource type contained within the created resource events.</typeparam>
    /// <returns>A new factory.</returns>
    public static ResourceEventDefinitionFactoryWithMapper<TResource> For<TResource>(Func<TResource> mapper) where TResource : ApiResource
    {
        return new ResourceEventDefinitionFactoryWithMapper<TResource>(mapper);
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionFactoryWithMapper{TResource}" /> with the specified query, to be
    /// used by client applications to then construct the individual <see cref="ResourceEventDefinitionFactoryWithMapper{TResource}" />
    /// instances without specifying the query multiple times.
    /// </summary>
    /// <param name="query">The self query, which is provided directly and useful when the query represents a singleton (typically within a given auth context, i.e. a CompanyApiResource for the current user).</param>
    /// <typeparam name="TResource">The resource type contained within the created resource events.</typeparam>
    /// <returns>A new factory.</returns>
    public static ResourceEventDefinitionFactoryWithMapper<TResource> For<TResource>(IQuery<TResource> query) where TResource : ApiResource
    {
        return new ResourceEventDefinitionFactoryWithMapper<TResource>(() => query);
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEventDefinitionFactory{TResource}" />.
    /// </summary>
    /// <typeparam name="TResource">The resource type contained within the created resource events.</typeparam>
    /// <returns>A new factory.</returns>
    public static ResourceEventDefinitionFactory<TResource> For<TResource>() where TResource : ApiResource
    {
        return new ResourceEventDefinitionFactory<TResource>();
    }
}

/// <summary>
/// Represents a unique event type within a system.
/// </summary>
/// <remarks>
/// Instead of having to define strongly-typed subclasses for every event in an API it is possible to
/// produce slightly less typed versions of events by defining your own classes with a list of
/// these definitions. They provide strong typing for the resource type, but loose typing for the
/// individual event types.
/// </remarks>
/// <typeparam name="TResource">The type of resource that has been modified.</typeparam>
public class ResourceEventDefinition<TResource> where TResource : ApiResource
{
    private protected readonly ResourceEventChangeType ChangeType;
    private protected readonly string EventId;

    /// <summary>
    /// Initialises a new instance of the <see cref="ResourceEventDefinition{TResource}" /> class.
    /// </summary>
    /// <param name="changeType">The type of change.</param>
    /// <param name="eventId">The specific event type.</param>
    public ResourceEventDefinition(ResourceEventChangeType changeType, string eventId)
    {
        this.ChangeType = changeType;
        this.EventId = eventId;
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEvent{TResource}" /> from this definition.
    /// </summary>
    /// <param name="resource">The resource this event is for.</param>
    /// <param name="metadata">Optional metadata that will be attached to the created resource event.</param>
    /// <returns>A new resource event.</returns>
    public ResourceEvent<TResource> New(TResource resource, [CanBeNull] IDictionary<string, object> metadata = null)
    {
        var resourceEvent = new ResourceEvent<TResource>(this.ChangeType, this.EventId, resource);

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                resourceEvent.WithMetadata(kvp.Key, kvp.Value);
            }
        }

        return resourceEvent;
    }

    /// <summary>
    /// Checks whether the given <see cref="ResourceEvent" /> matches events that would be created by
    /// this definition.
    /// </summary>
    /// <param name="event">The event to check.</param>
    /// <returns>Whether the event matches.</returns>
    public bool Matches(ResourceEvent @event)
    {
        return @event.ChangeType == this.ChangeType &&
               @event.EventId == this.EventId &&
               @event.ResourceObject == ApiResource.GetTypeName(typeof(TResource));
    }
}

/// <summary>
/// Represents a unique event type within a system.
/// </summary>
/// <remarks>
/// Instead of having to define strongly-typed subclasses for every event in an API it is possible to
/// produce slightly less typed versions of events by defining your own classes with a list of
/// these definitions. They provide strong typing for the resource type, but loose typing for the
/// individual event types.
/// </remarks>
/// <typeparam name="TResource">The type of resource that has been modified.</typeparam>
public class ResourceEventDefinitionWithMapper<TResource> : ResourceEventDefinition<TResource> where TResource : ApiResource
{
    [CanBeNull]
    private readonly Func<TResource> _mapper;

    [CanBeNull]
    private readonly Func<IQuery<TResource>> _queryMapper;

    /// <summary>
    /// Initialises a new instance of the <see cref="ResourceEventDefinition{TResource}" /> class.
    /// </summary>
    /// <param name="changeType">The type of change.</param>
    /// <param name="eventId">The specific event type.</param>
    /// <param name="queryMapper">A mapper to construct a "self" API operation.</param>
    public ResourceEventDefinitionWithMapper(ResourceEventChangeType changeType, string eventId, Func<IQuery<TResource>> queryMapper)
        : base(changeType, eventId)
    {
        this._queryMapper = queryMapper;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ResourceEventDefinition{TResource}" /> class.
    /// </summary>
    /// <param name="changeType">The type of change.</param>
    /// <param name="eventId">The specific event type.</param>
    /// <param name="mapper">A mapper to construct the final resource.</param>
    public ResourceEventDefinitionWithMapper(ResourceEventChangeType changeType, string eventId, Func<TResource> mapper)
        : base(changeType, eventId)
    {
        this._mapper = mapper;
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEvent{TResource}" /> from this definition.
    /// </summary>
    /// <param name="metadata">Optional metadata that will be attached to the created resource event.</param>
    /// <returns>A new resource event.</returns>
    public ResourceEvent<TResource> New([CanBeNull] IDictionary<string, object> metadata = null)
    {
        var resourceEvent = this._mapper != null ?
            new ResourceEvent<TResource>(this.ChangeType, this.EventId, this._mapper()) :
            new ResourceEvent<TResource>(this.ChangeType, this.EventId, this._queryMapper!());

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                resourceEvent.WithMetadata(kvp.Key, kvp.Value);
            }
        }

        return resourceEvent;
    }
}

/// <summary>
/// Represents a unique event type within a system.
/// </summary>
/// <remarks>
/// Instead of having to define strongly-typed subclasses for every event in an API it is possible to
/// produce slightly less typed versions of events by defining your own classes with a list of
/// these definitions. They provide strong typing for the resource type, but loose typing for the
/// individual event types.
/// </remarks>
/// <typeparam name="TResource">The type of resource that has been modified.</typeparam>
/// <typeparam name="TDomain">A "domain" type that will be passed in to a mapper when creating an instance from
/// this definition.</typeparam>
public sealed class ResourceEventDefinitionWithMapper<TResource, TDomain> : ResourceEventDefinition<TResource> where TResource : ApiResource
{
    [CanBeNull]
    private readonly Func<TDomain, TResource> _mapper;

    [CanBeNull]
    private readonly Func<TDomain, IQuery<TResource>> _queryMapper;

    /// <summary>
    /// Initialises a new instance of the <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" /> class.
    /// </summary>
    /// <param name="changeType">The type of change.</param>
    /// <param name="eventId">The specific event type.</param>
    /// <param name="queryMapper">A mapper to construct a "self" API operation.</param>
    public ResourceEventDefinitionWithMapper(ResourceEventChangeType changeType, string eventId, Func<TDomain, IQuery<TResource>> queryMapper)
        : base(changeType, eventId)
    {
        this._queryMapper = queryMapper;
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ResourceEventDefinitionWithMapper{TResource, TDomain}" /> class.
    /// </summary>
    /// <param name="changeType">The type of change.</param>
    /// <param name="eventId">The specific event type.</param>
    /// <param name="mapper">A mapper to construct a "self" API operation.</param>
    public ResourceEventDefinitionWithMapper(ResourceEventChangeType changeType, string eventId, Func<TDomain, TResource> mapper)
        : base(changeType, eventId)
    {
        this._mapper = mapper;
    }

    /// <summary>
    /// Creates a new <see cref="ResourceEvent{TResource}" /> from this definition, with it's "self query" created
    /// by passing the given <paramref name="domainObject" /> to the mapper of this definition.
    /// </summary>
    /// <param name="domainObject">The "domain object" to pass to the self-query mapper.</param>
    /// <param name="metadata">Optional metadata that will be attached to the created resource event.</param>
    /// <returns>A new resource event.</returns>
    public ResourceEvent<TResource> New(TDomain domainObject, [CanBeNull] IDictionary<string, object> metadata = null)
    {
        var resourceEvent = this._mapper != null
            ? new ResourceEvent<TResource>(this.ChangeType, this.EventId, this._mapper(domainObject))
            : new ResourceEvent<TResource>(this.ChangeType, this.EventId, this._queryMapper!(domainObject));

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                resourceEvent.WithMetadata(kvp.Key, kvp.Value);
            }
        }

        return resourceEvent;
    }
}