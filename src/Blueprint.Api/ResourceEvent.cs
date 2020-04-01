using System;
using System.Collections.Generic;
using Blueprint.Core;
using MediatR;
using Microsoft.CodeAnalysis.Differencing;
using Newtonsoft.Json;

namespace Blueprint.Api
{
    /// <summary>
    /// Represents an event that has happened within an API that has resulted in the modification, creation or
    /// deletion of an API resource (see <seealso cref="ApiResource" />).
    /// </summary>
    /// <remarks>
    /// Commands within an API can return a new <see cref="ResourceEvent" /> that represents a change, specifying
    /// the type of the change and how to load the associated resource (through another <see cref="IApiOperation" />)
    /// to present a common schema for indicating changes have occurred within a system and providing clients
    /// both internal and external to the API a way of reacting to events (i.e. by removing from cache if a
    /// <see cref="ResourceEventChangeType.Deleted" /> event is returned.
    /// </remarks>
    public class ResourceEvent : INotification
    {
        private readonly DateTimeOffset created;

        private readonly ResourceEventChangeType changeType;
        private readonly string eventId;
        private readonly Type resourceType;
        private readonly IApiOperation selfQuery;

        private readonly Dictionary<string, object> metadata = new Dictionary<string, object>();
        private readonly Dictionary<string, object> secureData = new Dictionary<string, object>();

        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEvent" /> class.
        /// </summary>
        /// <param name="changeType">The type of change.</param>
        /// <param name="eventId">The id of this specific event.</param>
        /// <param name="resourceType">The type of resource that has been modified.</param>
        /// <param name="selfQuery">An operation that, when executed, will load the associated resource.</param>
        public ResourceEvent(ResourceEventChangeType changeType, string eventId, Type resourceType, IApiOperation selfQuery)
        {
            Guard.EnumDefined(nameof(changeType), changeType);
            Guard.NotNull(nameof(resourceType), resourceType);
            Guard.NotNull(nameof(selfQuery), selfQuery);

            this.changeType = changeType;
            this.eventId = eventId;
            this.resourceType = resourceType;
            this.selfQuery = selfQuery;

            created = SystemTime.UtcNow;
        }

        /// <summary>
        /// Gets the object of this 'resource', which is "event".
        /// </summary>
        [JsonProperty(PropertyName = "$object")]
        public string Object => "event";

        /// <summary>
        /// Gets the id of this event, for example 'timeEntry.updated' or 'account.approvals.enabled'.
        /// </summary>
        /// <remarks>
        /// The id <em>should</em> be a dot-delimited string with the first part being the resource type of
        /// the object that has been altered.
        /// </remarks>
        public string EventId => eventId;

        /// <summary>
        /// Gets a the change type of this event, which is a higher-level version of the Type property,
        /// indicating whether the resource was updated, created or deleted.
        /// </summary>
        public ResourceEventChangeType ChangeType => changeType;

        /// <summary>
        /// Gets the created date of this event.
        /// </summary>
        public DateTimeOffset Created => created;

        /// <summary>
        /// Gets the query that represents the query that will load the resource this
        /// event represents.
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IApiOperation SelfQuery => selfQuery;

        /// <summary>
        /// Gets the type of resource represented.
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Type ResourceType => resourceType;

        /// <summary>
        /// Gets the type of resource represented.
        /// </summary>
        public string ResourceObject => ApiResource.GetTypeName(ResourceType);

        /// <summary>
        /// Gets or sets the href of the resource this event represents, to be populated by the ResourceEventMiddleware
        /// component from the resource type and id definition object.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the payload of this event, the actual resource that this represents, to be populated by the ResourceEventMiddleware
        /// component from the resource type and id definition object.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of previous values, used to identify what has actually changed
        /// between two resources, keyed on the property name with the previous value of the resource
        /// being the value.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> ChangedValues { get; set; }

        /// <summary>
        /// Gets the metadata dictionary of this resource event, a simple bag of key value pairs
        /// of useful information that can be stored free-form against an event.
        /// </summary>
        public Dictionary<string, object> Metadata => metadata;

        /// <summary>
        /// Gets a dictionary of security-related data that will NOT be persisted anywhere.
        /// </summary>
        public Dictionary<string, object> SecureData => secureData;

        /// <summary>
        /// Gets or sets the correlation id for this event, which can be used to tie it back to
        /// the initial request that resulted in an event.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the operation that triggered the generation of this resource event.
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IApiOperation Operation { get; set; }

        /// <summary>
        /// Adds the given key value pair to this event's <see cref="Metadata" /> dictionary, which can be
        /// used to store pieces of information that can be exposed to other parts of the system and API consumers
        /// but <strong>MAY</strong> also be persisted (i.e. for auditing purposes).
        /// </summary>
        /// <param name="key">The key of the item to add.</param>
        /// <param name="value">The value of the item to add.</param>
        /// <returns>This <see cref="ResourceEvent"/> to support further chaining.</returns>
        public ResourceEvent WithMetadata(string key, object value)
        {
            metadata[key] = value;

            return this;
        }

        /// <summary>
        /// Adds the given key value pair to this event's <see cref="SecureData" /> dictionary, which can be
        /// used to store pieces of information that can be exposed to other parts of the system and API consumers
        /// but <strong>MUST NOT</strong> be persisted (i.e. for auditing purposes).
        /// </summary>
        /// <param name="key">The key of the item to add.</param>
        /// <param name="value">The value of the item to add.</param>
        /// <returns>This <see cref="ResourceEvent"/> to support further chaining.</returns>
        public ResourceEvent WithSecureData(string key, object value)
        {
            secureData[key] = value;

            return this;
        }
    }

    /// <summary>
    /// A typed subclass of <see cref="ResourceEvent" /> that redefines the <see cref="ResourceEvent.Data" /> type to
    /// be of a specific type, useful for better type safety when dealing with events throughout the code
    /// base.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the event.</typeparam>
    public class ResourceEvent<TResource> : ResourceEvent
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEvent{T}" /> class.
        /// </summary>
        /// <param name="changeType">The type of change.</param>
        /// <param name="eventId">The specific event type.</param>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        public ResourceEvent(
            ResourceEventChangeType changeType,
            string eventId,
            IApiOperation<TResource> selfQuery)
            : base(changeType, eventId, typeof(TResource), selfQuery)
        {
        }

        /// <inherit-doc />
        // System.Text.Json needs explicit ignore of this overriden property. It _will_ still serialise the base
        [System.Text.Json.Serialization.JsonIgnore]
        public new TResource Data
        {
            get => (TResource)base.Data;
            set => base.Data = value;
        }

        /// <summary>
        /// Creates a fully-qualified event id, one that has the type name of the <see cref="ApiResource" />
        /// represented by <typeparamref name="TResource" />.
        /// </summary>
        /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
        /// <returns>A fully-qualified event id.</returns>
        public static string CreateId(string eventSubId)
        {
            return ApiResource.GetTypeName(typeof(TResource)) + "." + eventSubId;
        }
    }

    /// <summary>
    /// A specific implementation of <see cref="ResourceEvent{T}" /> for events of change type <see cref="ResourceEventChangeType.Created" />.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the event.</typeparam>
    public class ResourceCreated<TResource> : ResourceEvent<TResource>
    {
        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.created' with a ChangeType of 'created'.
        /// </summary>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        public ResourceCreated(IApiOperation<TResource> selfQuery)
            : base(ResourceEventChangeType.Created, CreateId("created"), selfQuery)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.created' with a ChangeType of 'created'.
        /// </summary>
        /// <param name="eventSubId">The name of this event, which is what gets put after `resourceTypeName.`, and
        /// should represent the action taken (for example 'timer.started' for a time entry would result in a full
        /// event name of `timeEntry.timer.started`).</param>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        public ResourceCreated(string eventSubId, IApiOperation<TResource> selfQuery)
            : base(ResourceEventChangeType.Created,  CreateId(eventSubId), selfQuery)
        {
        }
    }

    /// <summary>
    /// A specific implementation of <see cref="ResourceEvent{T}" /> for events of change type <see cref="ResourceEventChangeType.Updated" />.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the event.</typeparam>
    public class ResourceUpdated<TResource> : ResourceEvent<TResource>
    {
        /// <summary>
        /// Creates a new <see cref="ResourceCreated{TResource}" /> instance, creating a default
        /// event type of '`resourceTypeName`.updated' with a ChangeType of 'updated'.
        /// </summary>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        public ResourceUpdated(IApiOperation<TResource> selfQuery)
            : base(ResourceEventChangeType.Updated, CreateId("updated"), selfQuery)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, setting the event type
        /// to '`resourceTypeName`.`eventName`' with a ChangeType of 'updated'.
        /// </summary>
        /// <param name="eventSubId">The name of this event, which is what gets put after `resourceTypeName.`, and
        /// should represent the action taken (for example 'approved', or 'rate.updated').</param>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        protected ResourceUpdated(string eventSubId, IApiOperation<TResource> selfQuery)
            : base(ResourceEventChangeType.Updated,  CreateId(eventSubId), selfQuery)
        {
        }
    }

    /// <summary>
    /// A specific implementation of <see cref="ResourceEvent{T}" /> for events of change type <see cref="ResourceEventChangeType.Deleted" />.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the event.</typeparam>
    public class ResourceDeleted<TResource> : ResourceEvent<TResource>
    {
        /// <summary>
        /// Creates a new <see cref="ResourceDeleted{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.deleted' with a ChangeType of 'deleted'.
        /// </summary>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        public ResourceDeleted(IApiOperation<TResource> selfQuery)
            : base(ResourceEventChangeType.Deleted, CreateId("deleted"), selfQuery)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceDeleted{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.deleted' with a ChangeType of 'deleted'.
        /// </summary>
        /// <param name="eventName">The name of this event, overriding the default of 'deleted'.</param>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        public ResourceDeleted(string eventName, IApiOperation<TResource> selfQuery)
            : base(ResourceEventChangeType.Deleted, CreateId(eventName), selfQuery)
        {
        }
    }
}
