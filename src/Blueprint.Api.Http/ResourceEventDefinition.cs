using System;

namespace Blueprint.Api.Http
{
    /// <summary>
    /// An entry point to constructing <see cref="ResourceEventDefinition{TResource, TDomain}" /> instances.
    /// </summary>
    public static class ResourceEventDefinition
    {
        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinitionFactory{TResource, TDomain}" /> with the specified mapper, to be
        /// used by client applications to then construct the individual <see cref="ResourceEventDefinition{TResource, TDomain}" />
        /// instances without specifying the mapper multiple times.
        /// </summary>
        /// <param name="mapper">The mapper use dto construct self query <see cref="IApiOperation"/> instances.</param>
        /// <typeparam name="TResource">The resource type contained within the created resource events.</typeparam>
        /// <typeparam name="TDomain">A "domain" type that will be passed in to a mapper when creating an instance from
        /// this definition.</typeparam>
        /// <returns>A new factory.</returns>
        public static ResourceEventDefinitionFactory<TResource, TDomain> For<TResource, TDomain>(Func<TDomain, IQuery<TResource>> mapper)
        {
            return new ResourceEventDefinitionFactory<TResource, TDomain>(mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinitionFactory{TResource, TDomain}" /> with the specified mapper, to be
        /// used by client applications to then construct the individual <see cref="ResourceEventDefinition{TResource, TDomain}" />
        /// instances without specifying the mapper multiple times.
        /// </summary>
        /// <param name="mapper">The mapper use dto construct self query <see cref="IApiOperation"/> instances.</param>
        /// <typeparam name="TResource">The resource type contained within the created resource events.</typeparam>
        /// <returns>A new factory.</returns>
        public static ResourceEventDefinitionFactory<TResource> For<TResource>(Func<IQuery<TResource>> mapper)
        {
            return new ResourceEventDefinitionFactory<TResource>(mapper);
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
    public sealed class ResourceEventDefinition<TResource>
    {
        private readonly ResourceEventChangeType changeType;
        private readonly string eventId;
        private readonly Func<IQuery<TResource>> mapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEventDefinition{TResource}" /> class.
        /// </summary>
        /// <param name="changeType">The type of change.</param>
        /// <param name="eventId">The specific event type.</param>
        /// <param name="mapper">A mapper to construct a "self" API operation.</param>
        public ResourceEventDefinition(ResourceEventChangeType changeType, string eventId, Func<IQuery<TResource>> mapper)
        {
            this.changeType = changeType;
            this.eventId = eventId;
            this.mapper = mapper;
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEvent{TResource}" /> from this definition.
        /// </summary>
        /// <returns>A new resource event.</returns>
        public ResourceEvent<TResource> New()
        {
            return new ResourceEvent<TResource>(changeType, eventId, mapper());
        }

        /// <summary>
        /// Checks whether the given <see cref="ResourceEvent" /> matches events that would be created by
        /// this definition.
        /// </summary>
        /// <param name="event">The event to check.</param>
        /// <returns>Whether the event matches.</returns>
        public bool Matches(ResourceEvent @event)
        {
            return @event.ChangeType == changeType &&
                   @event.EventId == eventId &&
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
    /// <typeparam name="TDomain">A "domain" type that will be passed in to a mapper when creating an instance from
    /// this definition.</typeparam>
    public sealed class ResourceEventDefinition<TResource, TDomain>
    {
        private readonly ResourceEventChangeType changeType;
        private readonly string eventId;
        private readonly Func<TDomain, IQuery<TResource>> mapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEventDefinition{TResource, TDomain}" /> class.
        /// </summary>
        /// <param name="changeType">The type of change.</param>
        /// <param name="eventId">The specific event type.</param>
        /// <param name="mapper">A mapper to construct a "self" API operation.</param>
        public ResourceEventDefinition(ResourceEventChangeType changeType, string eventId, Func<TDomain, IQuery<TResource>> mapper)
        {
            this.changeType = changeType;
            this.eventId = eventId;
            this.mapper = mapper;
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEvent{TResource}" /> from this definition, with it's "self query" created
        /// by passing the given <paramref name="domainObject" /> to the mapper of this definition.
        /// </summary>
        /// <param name="domainObject">The "domain object" to pass to the self-query mapper.</param>
        /// <returns>A new resource event.</returns>
        public ResourceEvent<TResource> New(TDomain domainObject)
        {
            return new ResourceEvent<TResource>(changeType, eventId, mapper(domainObject));
        }

        /// <summary>
        /// Checks whether the given <see cref="ResourceEvent" /> matches events that would be created by
        /// this definition.
        /// </summary>
        /// <param name="event">The event to check.</param>
        /// <returns>Whether the event matches.</returns>
        public bool Matches(ResourceEvent @event)
        {
            return @event.ChangeType == changeType &&
                   @event.EventId == eventId &&
                   @event.ResourceObject == ApiResource.GetTypeName(typeof(TResource));
        }
    }
}
