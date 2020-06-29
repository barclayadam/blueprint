using System.Reflection;
using Blueprint.Compiler.Model;

namespace Blueprint.Validation
{
    /// <summary>
    /// A <see cref="Variable"/> that represents access to a <see cref="PropertyInfo"/> value at runtime, carrying
    /// the actual info to be inspected at code build time.
    /// </summary>
    public class PropertyInfoVariable : Variable
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="PropertyInfoVariable"/> class.
        /// </summary>
        /// <param name="property">The property this variable represents at both runtime and compile time.</param>
        /// <param name="usage">How to access the property info at runtime.</param>
        public PropertyInfoVariable(PropertyInfo property, string usage) : base(typeof(PropertyInfo), usage)
        {
            Property = property;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> this variable represents.
        /// </summary>
        public PropertyInfo Property { get; }
    }
}
