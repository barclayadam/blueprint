namespace Blueprint.Core
{
    /// <summary>
    /// Represents an Entity, an object that has a physically persisted and unique representation
    /// in a backing store (database).
    /// </summary>
    /// <remarks>
    /// An entity may or may not be an aggregate root, the only defining characteristic is that it has
    /// its own identity (e.g. not a value object).
    /// </remarks>
    public abstract class Entity
    {
        /// <summary>
        /// Gets or sets the Id of this entity, which when creating a new entity will be a new integer
        /// as set in the constructor.
        /// </summary>
        public virtual int Id { get; protected set; }

        /// <summary>
        /// Indicates whether this entity is equal to the other given entity, which is defined
        /// as being the equality of the two Ids (e.g. two entities with different values but the
        /// same Id will be considered as equal).
        /// </summary>
        /// <param name="other">The other entity to check for equality.</param>
        /// <returns>Whether the other entity is the same as this one.</returns>
        public virtual bool Equals(Entity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.Id == Id;
        }

        /// <summary>
        /// Indicates whether or not the given object is the same as this Entity.
        /// </summary>
        /// <param name="obj">The object to check for equality.</param>
        /// <returns>Whether the given object is the same as this entity.</returns>
        /// <seealso cref="Equals(Blueprint.Core.Entity)"/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var otherEntity = obj as Entity;

            return otherEntity != null && Equals(otherEntity);
        }

        /// <summary>
        /// Gets the hash code of this entity, which is defined as the hash code of its
        /// <see cref="Id"/>.
        /// </summary>
        /// <returns>The id of this entity.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}