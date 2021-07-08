using System;

namespace Blueprint.Http
{
    /// <summary>
    /// A factory to create instances of <see cref="ResourceEventDefinition{TResource}" /> without having to
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
    public class ResourceEventDefinitionFactory<TResource> where TResource : ApiResource
    {
        private readonly Func<IQuery<TResource>> _mapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEventDefinitionFactory{TResource,TDomain}" /> class.
        /// </summary>
        /// <param name="mapper">A mapper to construct a "self" API operation.</param>
        public ResourceEventDefinitionFactory(Func<IQuery<TResource>> mapper)
        {
            this._mapper = mapper;
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Created" /> and an event sub id of <c>created</c>.
        /// </summary>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource> Created()
        {
            return new ResourceEventDefinition<TResource>(
                ResourceEventChangeType.Created,
                ResourceEvent<TResource>.CreateId("created"),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Created" /> and the given event sub id.
        /// </summary>
        /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource> Created(string eventSubId)
        {
            return new ResourceEventDefinition<TResource>(
                ResourceEventChangeType.Created,
                ResourceEvent<TResource>.CreateId(eventSubId),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Updated" /> and an event sub id of <c>updated</c>.
        /// </summary>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource> Updated()
        {
            return new ResourceEventDefinition<TResource>(
                ResourceEventChangeType.Updated,
                ResourceEvent<TResource>.CreateId("updated"),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Updated" /> and the given event sub id.
        /// </summary>
        /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource> Updated(string eventSubId)
        {
            return new ResourceEventDefinition<TResource>(
                ResourceEventChangeType.Updated,
                ResourceEvent<TResource>.CreateId(eventSubId),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Deleted" /> and an event sub id of <c>deleted</c>.
        /// </summary>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource> Deleted()
        {
            return new ResourceEventDefinition<TResource>(
                ResourceEventChangeType.Deleted,
                ResourceEvent<TResource>.CreateId("deleted"),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Deleted" /> and the given event sub id.
        /// </summary>
        /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource> Deleted(string eventSubId)
        {
            return new ResourceEventDefinition<TResource>(
                ResourceEventChangeType.Deleted,
                ResourceEvent<TResource>.CreateId(eventSubId),
                this._mapper);
        }
    }

    /// <summary>
    /// A factory to create instances of <see cref="ResourceEventDefinition{TResource, TDomain}" /> without having to
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
    public class ResourceEventDefinitionFactory<TResource, TDomain> where TResource : ApiResource
    {
        private readonly Func<TDomain, IQuery<TResource>> _mapper;

        /// <summary>
        /// Initialises a new instance of the <see cref="ResourceEventDefinitionFactory{TResource,TDomain}" /> class.
        /// </summary>
        /// <param name="mapper">A mapper to construct a "self" API operation.</param>
        public ResourceEventDefinitionFactory(Func<TDomain, IQuery<TResource>> mapper)
        {
            this._mapper = mapper;
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Created" /> and an event sub id of <c>created</c>.
        /// </summary>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource, TDomain> Created()
        {
            return new ResourceEventDefinition<TResource, TDomain>(
                ResourceEventChangeType.Created,
                ResourceEvent<TResource>.CreateId("created"),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Created" /> and the given event sub id.
        /// </summary>
        /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource, TDomain> Created(string eventSubId)
        {
            return new ResourceEventDefinition<TResource, TDomain>(
                ResourceEventChangeType.Created,
                ResourceEvent<TResource>.CreateId(eventSubId),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Updated" /> and an event sub id of <c>updated</c>.
        /// </summary>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource, TDomain> Updated()
        {
            return new ResourceEventDefinition<TResource, TDomain>(
                ResourceEventChangeType.Updated,
                ResourceEvent<TResource>.CreateId("updated"),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Updated" /> and the given event sub id.
        /// </summary>
        /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource, TDomain> Updated(string eventSubId)
        {
            return new ResourceEventDefinition<TResource, TDomain>(
                ResourceEventChangeType.Updated,
                ResourceEvent<TResource>.CreateId(eventSubId),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Deleted" /> and an event sub id of <c>deleted</c>.
        /// </summary>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource, TDomain> Deleted()
        {
            return new ResourceEventDefinition<TResource, TDomain>(
                ResourceEventChangeType.Deleted,
                ResourceEvent<TResource>.CreateId("deleted"),
                this._mapper);
        }

        /// <summary>
        /// Creates a new <see cref="ResourceEventDefinition{TResource, TDomain}" /> with a change type
        /// of <see cref="ResourceEventChangeType.Deleted" /> and the given event sub id.
        /// </summary>
        /// <param name="eventSubId">The "sub" ID, unique within a resource's event namespace.</param>
        /// <returns>A new resource event definition.</returns>
        public ResourceEventDefinition<TResource, TDomain> Deleted(string eventSubId)
        {
            return new ResourceEventDefinition<TResource, TDomain>(
                ResourceEventChangeType.Deleted,
                ResourceEvent<TResource>.CreateId(eventSubId),
                this._mapper);
        }
    }
}
