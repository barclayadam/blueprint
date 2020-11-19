using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Blueprint.Http
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
        /// <param name="mapper">The mapper used to construct self query operation instances.</param>
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
        /// <param name="mapper">The mapper used to construct self query operation instances.</param>
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
        private readonly ResourceEventChangeType _changeType;
        private readonly string _eventId;
        private readonly Func<IQuery<TResource>> _mapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEventDefinition{TResource}" /> class.
        /// </summary>
        /// <param name="changeType">The type of change.</param>
        /// <param name="eventId">The specific event type.</param>
        /// <param name="mapper">A mapper to construct a "self" API operation.</param>
        public ResourceEventDefinition(ResourceEventChangeType changeType, string eventId, Func<IQuery<TResource>> mapper)
        {
            this._changeType = changeType;
            this._eventId = eventId;
            this._mapper = mapper;
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEvent{TResource}" /> from this definition.
        /// </summary>
        /// <param name="metadata">Optional metadata that will be attached to the created resource event.</param>
        /// <returns>A new resource event.</returns>
        public ResourceEvent<TResource> New([CanBeNull] IDictionary<string, object> metadata = null)
        {
            var resourceEvent = new ResourceEvent<TResource>(this._changeType, this._eventId, this._mapper());

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
            return @event.ChangeType == this._changeType &&
                   @event.EventId == this._eventId &&
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
        private readonly ResourceEventChangeType _changeType;
        private readonly string _eventId;
        private readonly Func<TDomain, IQuery<TResource>> _mapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEventDefinition{TResource, TDomain}" /> class.
        /// </summary>
        /// <param name="changeType">The type of change.</param>
        /// <param name="eventId">The specific event type.</param>
        /// <param name="mapper">A mapper to construct a "self" API operation.</param>
        public ResourceEventDefinition(ResourceEventChangeType changeType, string eventId, Func<TDomain, IQuery<TResource>> mapper)
        {
            this._changeType = changeType;
            this._eventId = eventId;
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
            var resourceEvent = new ResourceEvent<TResource>(this._changeType, this._eventId, this._mapper(domainObject));

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
            return @event.ChangeType == this._changeType &&
                   @event.EventId == this._eventId &&
                   @event.ResourceObject == ApiResource.GetTypeName(typeof(TResource));
        }
    }
}
