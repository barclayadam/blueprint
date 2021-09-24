using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Blueprint.Http
{
    /// <summary>
    /// Represents an event that has happened within an API that has resulted in the modification, creation or
    /// deletion of an API resource (see <seealso cref="ApiResource" />).
    /// </summary>
    /// <remarks>
    /// Commands within an API can return a new <see cref="ResourceEvent" /> that represents a change, specifying
    /// the type of the change and how to load the associated resource (through another operation that <b>MUST</b> be of
    /// type <see cref="IQuery" />)
    /// to present a common schema for indicating changes have occurred within a system and providing clients
    /// both internal and external to the API a way of reacting to events (i.e. by removing from cache if a
    /// <see cref="ResourceEventChangeType.Deleted" /> event is returned.
    /// </remarks>
    public class ResourceEvent : IApiResourceCollection
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEvent" /> class.
        /// </summary>
        /// <param name="changeType">The type of change.</param>
        /// <param name="eventId">The id of this specific event.</param>
        /// <param name="resourceType">The type of resource that has been modified.</param>
        /// <param name="selfQuery">An operation that, when executed, will load the associated resource.</param>
        public ResourceEvent(ResourceEventChangeType changeType, string eventId, Type resourceType, IQuery selfQuery)
        {
            Guard.EnumDefined(nameof(changeType), changeType);
            Guard.NotNull(nameof(resourceType), resourceType);
            Guard.NotNull(nameof(selfQuery), selfQuery);

            this.ChangeType = changeType;
            this.EventId = eventId;
            this.ResourceType = resourceType;
            this.ResourceObject = ApiResource.GetTypeName(resourceType);
            this.SelfQuery = selfQuery;

            this.Created = SystemTime.UtcNow;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEvent" /> class.
        /// </summary>
        /// <param name="changeType">The type of change.</param>
        /// <param name="eventId">The id of this specific event.</param>
        /// <param name="data">The API resource this event applies to.</param>
        [System.Text.Json.Serialization.JsonConstructor]
        public ResourceEvent(ResourceEventChangeType changeType, string eventId, object data)
        {
            Guard.EnumDefined(nameof(changeType), changeType);
            Guard.NotNull(nameof(data), data);

            this.ChangeType = changeType;
            this.EventId = eventId;
            this.ResourceType = data.GetType();
            this.ResourceObject = ApiResource.GetTypeName(this.ResourceType);
            this.Data = data;

            this.Created = SystemTime.UtcNow;
        }

        /// <summary>
        /// Gets the object of this 'resource', which is "event".
        /// </summary>
        [JsonProperty(PropertyName = "$object")]
        [JsonPropertyName("$object")]
        [JsonInclude]
        public string Object => "event";

        /// <summary>
        /// Gets the id of this event, for example 'timeEntry.updated' or 'account.approvals.enabled'.
        /// </summary>
        /// <remarks>
        /// The id <em>should</em> be a dot-delimited string with the first part being the resource type of
        /// the object that has been altered.
        /// </remarks>
        [JsonInclude]
        public string EventId { get; internal set; }

        /// <summary>
        /// Gets a the change type of this event, which is a higher-level version of the Type property,
        /// indicating whether the resource was updated, created or deleted.
        /// </summary>
        [JsonInclude]
        public ResourceEventChangeType ChangeType { get; internal set; }

        /// <summary>
        /// Gets the created date of this event.
        /// </summary>
        [JsonInclude]
        public DateTimeOffset Created { get; internal set; }

        /// <summary>
        /// Gets the query that represents the query that will load the resource this
        /// event represents.
        /// </summary>
        [global::Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public IQuery SelfQuery { get; }

        /// <summary>
        /// Gets the type of resource represented.
        /// </summary>
        [JsonInclude]
        public string ResourceObject { get; internal set; }

        /// <summary>
        /// Gets or sets the href of the resource this event represents, to be populated by the ResourceEventMiddleware
        /// component from the resource type and id definition object.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonInclude]
        public string Href { get; set; }

        /// <summary>
        /// Gets or sets the payload of this event, the actual resource that this represents, to be populated by the ResourceEventMiddleware
        /// component from the resource type and id definition object.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonInclude]
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of previous values, used to identify what has actually changed
        /// between two resources, keyed on the property name with the previous value of the resource
        /// being the value.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonInclude]
        public Dictionary<string, object> ChangedValues { get; set; }

        /// <summary>
        /// Gets the metadata dictionary of this resource event, a simple bag of key value pairs
        /// of useful information that can be stored free-form against an event.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, object> Metadata { get; internal set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets a dictionary of security-related data that will NOT be persisted anywhere.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, object> SecureData { get; internal set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the correlation id for this event, which can be used to tie it back to
        /// the initial request that resulted in an event.
        /// </summary>
        [JsonInclude]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the operation that triggered the generation of this resource event.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public object Operation { get; set; }

        /// <summary>
        /// The type this resource event represents, internal to allow only using locally and to not confuse
        /// clients given we will <b>not</b> deserialise this value.
        /// </summary>
        [global::Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        internal Type ResourceType { get; set; }

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
            this.Metadata[key] = value;

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
            this.SecureData[key] = value;

            return this;
        }

        /// <inheritdoc/>
        IEnumerable<object> IApiResourceCollection.GetEnumerable()
        {
            yield return this.Data;
        }
    }

    /// <summary>
    /// A typed subclass of <see cref="ResourceEvent" /> that redefines the <see cref="ResourceEvent.Data" /> type to
    /// be of a specific type, useful for better type safety when dealing with events throughout the code
    /// base.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the event.</typeparam>
    public class ResourceEvent<TResource> : ResourceEvent where TResource : ApiResource
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
            IQuery<TResource> selfQuery)
            : base(changeType, eventId, typeof(TResource), selfQuery)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEvent{T}" /> class.
        /// </summary>
        /// <param name="changeType">The type of change.</param>
        /// <param name="eventId">The specific event type.</param>
        /// <param name="data">The resource this event is for.</param>
        [System.Text.Json.Serialization.JsonConstructor]
        public ResourceEvent(
            ResourceEventChangeType changeType,
            string eventId,
            TResource data)
            : base(changeType, eventId, data)
        {
        }

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

        /// <summary>
        /// Adds the given key value pair to this event's <see cref="ResourceEvent.Metadata" /> dictionary, which can be
        /// used to store pieces of information that can be exposed to other parts of the system and API consumers
        /// but <strong>MAY</strong> also be persisted (i.e. for auditing purposes).
        /// </summary>
        /// <param name="key">The key of the item to add.</param>
        /// <param name="value">The value of the item to add.</param>
        /// <returns>This <see cref="ResourceEvent"/> to support further chaining.</returns>
        public new ResourceEvent<TResource> WithMetadata(string key, object value)
        {
            base.WithMetadata(key, value);

            return this;
        }

        /// <summary>
        /// Adds the given key value pair to this event's <see cref="ResourceEvent.SecureData" /> dictionary, which can be
        /// used to store pieces of information that can be exposed to other parts of the system and API consumers
        /// but <strong>MUST NOT</strong> be persisted (i.e. for auditing purposes).
        /// </summary>
        /// <param name="key">The key of the item to add.</param>
        /// <param name="value">The value of the item to add.</param>
        /// <returns>This <see cref="ResourceEvent"/> to support further chaining.</returns>
        public new ResourceEvent<TResource> WithSecureData(string key, object value)
        {
            base.WithSecureData(key, value);

            return this;
        }
    }

    /// <summary>
    /// A specific implementation of <see cref="ResourceEvent{T}" /> for events of change type <see cref="ResourceEventChangeType.Created" />.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the event.</typeparam>
    public class ResourceCreated<TResource> : ResourceEvent<TResource> where TResource : ApiResource
    {
        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.created' with a ChangeType of 'created'.
        /// </summary>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        public ResourceCreated(IQuery<TResource> selfQuery)
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
        public ResourceCreated(string eventSubId, IQuery<TResource> selfQuery)
            : base(ResourceEventChangeType.Created,  CreateId(eventSubId), selfQuery)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.created' with a ChangeType of 'created'.
        /// </summary>
        /// <param name="data">The resource this event is for.</param>
        public ResourceCreated(TResource data)
            : base(ResourceEventChangeType.Created, CreateId("created"), data)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.created' with a ChangeType of 'created'.
        /// </summary>
        /// <param name="eventSubId">The name of this event, which is what gets put after `resourceTypeName.`, and
        /// should represent the action taken (for example 'timer.started' for a time entry would result in a full
        /// event name of `timeEntry.timer.started`).</param>
        /// <param name="data">The resource this event is for.</param>
        [System.Text.Json.Serialization.JsonConstructor]
        public ResourceCreated(string eventSubId, TResource data)
            : base(ResourceEventChangeType.Created,  CreateId(eventSubId), data)
        {
        }
    }

    /// <summary>
    /// A specific implementation of <see cref="ResourceEvent{T}" /> for events of change type <see cref="ResourceEventChangeType.Updated" />.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the event.</typeparam>
    public class ResourceUpdated<TResource> : ResourceEvent<TResource> where TResource : ApiResource
    {
        /// <summary>
        /// Creates a new <see cref="ResourceCreated{TResource}" /> instance, creating a default
        /// event type of '`resourceTypeName`.updated' with a ChangeType of 'updated'.
        /// </summary>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        public ResourceUpdated(IQuery<TResource> selfQuery)
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
        [System.Text.Json.Serialization.JsonConstructor]
        protected ResourceUpdated(string eventSubId, IQuery<TResource> selfQuery)
            : base(ResourceEventChangeType.Updated,  CreateId(eventSubId), selfQuery)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceCreated{TResource}" /> instance, creating a default
        /// event type of '`resourceTypeName`.updated' with a ChangeType of 'updated'.
        /// </summary>
        /// <param name="data">The resource this event is for.</param>
        public ResourceUpdated(TResource data)
            : base(ResourceEventChangeType.Updated, CreateId("updated"), data)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, setting the event type
        /// to '`resourceTypeName`.`eventName`' with a ChangeType of 'updated'.
        /// </summary>
        /// <param name="eventSubId">The name of this event, which is what gets put after `resourceTypeName.`, and
        /// should represent the action taken (for example 'approved', or 'rate.updated').</param>
        /// <param name="data">The resource this event is for.</param>
        public ResourceUpdated(string eventSubId, TResource data)
            : base(ResourceEventChangeType.Updated,  CreateId(eventSubId), data)
        {
        }
    }

    /// <summary>
    /// A specific implementation of <see cref="ResourceEvent{T}" /> for events of change type <see cref="ResourceEventChangeType.Deleted" />.
    /// </summary>
    /// <typeparam name="TResource">The type of resource contained within the event.</typeparam>
    public class ResourceDeleted<TResource> : ResourceEvent<TResource> where TResource : ApiResource
    {
        /// <summary>
        /// Creates a new <see cref="ResourceDeleted{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.deleted' with a ChangeType of 'deleted'.
        /// </summary>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        public ResourceDeleted(IQuery<TResource> selfQuery)
            : base(ResourceEventChangeType.Deleted, CreateId("deleted"), selfQuery)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceDeleted{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.deleted' with a ChangeType of 'deleted'.
        /// </summary>
        /// <param name="eventName">The name of this event, overriding the default of 'deleted'.</param>
        /// <param name="selfQuery">An operation that, when executed, will get the resource this event represents.</param>
        [System.Text.Json.Serialization.JsonConstructor]
        public ResourceDeleted(string eventName, IQuery<TResource> selfQuery)
            : base(ResourceEventChangeType.Deleted, CreateId(eventName), selfQuery)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceDeleted{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.deleted' with a ChangeType of 'deleted'.
        /// </summary>
        /// <param name="data">The resource this event is for.</param>
        public ResourceDeleted(TResource data)
            : base(ResourceEventChangeType.Deleted, CreateId("deleted"), data)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceDeleted{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.deleted' with a ChangeType of 'deleted'.
        /// </summary>
        /// <param name="eventName">The name of this event, overriding the default of 'deleted'.</param>
        /// <param name="data">The resource this event is for.</param>
        public ResourceDeleted(string eventName, TResource data)
            : base(ResourceEventChangeType.Deleted, CreateId(eventName), data)
        {
        }
    }
}
