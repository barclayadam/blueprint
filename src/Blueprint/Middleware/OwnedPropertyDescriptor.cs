using System;
using System.Reflection;

namespace Blueprint.Middleware
{
    /// <summary>
    /// Describes an "owned" property as returned from <see cref="IMessagePopulationSource.GetOwnedProperties" />.
    /// </summary>
    public class OwnedPropertyDescriptor : IEquatable<OwnedPropertyDescriptor>
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="OwnedPropertyDescriptor" /> class.
        /// </summary>
        /// <param name="property">The property that is "owned"</param>
        public OwnedPropertyDescriptor(PropertyInfo property)
        {
            this.Property = property;
            this.PropertyName = property.Name;
        }

        /// <summary>
        /// The property that is "owned"
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// The name of the property, which is typically the same as <see cref="PropertyInfo.Name" />
        /// but may be overriden if the property should be exposed to users as something else.
        /// </summary>
        /// <remarks>
        /// This is useful in particular in HTTP APIs to override the name to be set in a cookie or
        /// header compared to the C# property name).
        /// </remarks>
        public string PropertyName { get; set; }

        /// <summary>
        /// Whether this property is considered internal, that it would come from internal state
        /// and should therefore NOT be exposed as a client-settable property (i.e. exclude from
        /// any OpenApi schemas).
        /// </summary>
        public bool IsInternal { get; set; }

        /// <inherit-doc />
        public bool Equals(OwnedPropertyDescriptor other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Property, other.Property);
        }

        /// <inherit-doc />
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

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((OwnedPropertyDescriptor) obj);
        }

        /// <inherit-doc />
        public override int GetHashCode()
        {
            return (Property != null ? Property.GetHashCode() : 0);
        }
    }
}
