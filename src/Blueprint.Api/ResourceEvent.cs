using System;
using System.Collections.Generic;
using Blueprint.Core;
using Newtonsoft.Json;

namespace Blueprint.Api
{
    public abstract class ResourceEvent
    {
        private readonly DateTimeOffset created;

        private readonly ResourceEventChangeType changeType;
        private readonly string eventType;
        private readonly Type resourceType;
        private readonly IApiOperation selfQuery;

        private readonly Dictionary<string, object> metadata = new Dictionary<string, object>();

        protected ResourceEvent(ResourceEventChangeType changeType, string eventType, Type resourceType, IApiOperation selfQuery)
        {
            Guard.EnumDefined("changeType", changeType);
            Guard.NotNull(nameof(resourceType), resourceType);
            Guard.NotNull(nameof(selfQuery), selfQuery);

            this.changeType = changeType;
            this.eventType = eventType;
            this.resourceType = resourceType;
            this.selfQuery = selfQuery;

            created = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Gets the object of this 'resource', which is "event".
        /// </summary>
        public string Object => "event";

        /// <summary>
        /// Gets the type of this event, for example 'timeEntry.updated' or 'account.approvals.enabled'.
        /// </summary>
        /// <remarks>
        /// The type should be a dot-delimited string with the first part being the resource type of
        /// the object that has been altered.
        /// </remarks>
        public string Type => eventType;

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
        public IApiOperation SelfQuery => selfQuery;

        /// <summary>
        /// Gets the type of resource represented.
        /// </summary>
        [JsonIgnore]
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
        /// of useful information that can be stored freeform against an event.
        /// </summary>
        public Dictionary<string, object> Metadata => metadata;

        /// <summary>
        /// Gets or sets the correlation id for this event, which can be used to tie it back to
        /// the initial request that resulted in an event.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the operation that triggered the generation of this resource event.
        /// </summary>
        [JsonIgnore]
        public IApiOperation Operation { get; set; }

        public ResourceEvent WithMetadata(string key, object value)
        {
            metadata[key] = value;

            return this;
        }
    }

    public class ResourceCreated<T> : ResourceEvent
    {
        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.created' with a ChangeType of 'created'.
        /// </summary>
        /// <param name="selfQuery">An object with properties required to create a canonical link
        /// of the given resource type, as defined by it's 'self' link.</param>
        public ResourceCreated(IApiOperation selfQuery)
            : base(ResourceEventChangeType.Created, ApiResource.GetTypeName(typeof(T)) + ".created", typeof(T), selfQuery)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.created' with a ChangeType of 'created'.
        /// </summary>
        /// <param name="eventName">The name of this event, which is what gets put after `resourceTypeName.`, and
        /// should represent the action taken (for example 'timer.started').</param>
        /// <param name="selfQuery">An object with properties required to create a canonical link
        /// of the given resource type, as defined by it's 'self' link.</param>
        public ResourceCreated(string eventName, IApiOperation selfQuery)
            : base(ResourceEventChangeType.Created, ApiResource.GetTypeName(typeof(T)) + "." + eventName, typeof(T), selfQuery)
        {
        }

        public new T Data
        {
            get => (T)base.Data;
            set => base.Data = value;
        }
    }

    public abstract class ResourceUpdated<T> : ResourceEvent
    {
        /// <summary>
        /// Creates a new <see cref="ResourceCreated{T}" /> instance, setting the event type
        /// to '`resourceTypeName`.`eventName`' with a ChangeType of 'updated'.
        /// </summary>
        /// <param name="eventName">The name of this event, which is what gets put after `resourceTypeName.`, and
        /// should represent the action taken (for example 'approved', or 'rate.updated').</param>
        /// <param name="selfQuery">An object with properties required to create a canonical link
        /// of the given resource type, as defined by it's 'self' link.</param>
        protected ResourceUpdated(string eventName, IApiOperation selfQuery)
            : base(ResourceEventChangeType.Updated, ApiResource.GetTypeName(typeof(T)) + "." + eventName, typeof(T), selfQuery)
        {
        }

        public new T Data
        {
            get => (T)base.Data;
            set => base.Data = value;
        }
    }

    public class ResourceDeleted<T> : ResourceEvent
    {
        /// <summary>
        /// Creates a new <see cref="ResourceDeleted{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.deleted' with a ChangeType of 'deleted'.
        /// </summary>
        /// <param name="selfQuery">An object with properties required to create a canonical link
        /// of the given resource type, as defined by it's 'self' link.</param>
        public ResourceDeleted(IApiOperation selfQuery)
            : base(ResourceEventChangeType.Deleted, ApiResource.GetTypeName(typeof(T)) + ".deleted", typeof(T), selfQuery)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ResourceDeleted{T}" /> instance, creating a default
        /// event type of '`resourceTypeName`.deleted' with a ChangeType of 'deleted'.
        /// </summary>
        /// <param name="eventName">The name of this event, overriding the default of 'deleted'.</param>
        /// <param name="selfQuery">An object with properties required to create a canonical link
        /// of the given resource type, as defined by it's 'self' link.</param>
        public ResourceDeleted(string eventName, IApiOperation selfQuery)
            : base(ResourceEventChangeType.Deleted, ApiResource.GetTypeName(typeof(T)) + "." + eventName, typeof(T), selfQuery)
        {
        }

        public new T Data
        {
            get => (T)base.Data;
            set => base.Data = value;
        }
    }
}
