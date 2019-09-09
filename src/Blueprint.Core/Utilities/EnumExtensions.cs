using System;
using System.ComponentModel;
using System.Linq;

namespace Blueprint.Core.Utilities
{
    /// <summary>
    /// Provides extension methods to enumeration values.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description of a particular enumeration value, using a
        /// <see cref="DescriptionAttribute"/> and its value should one exist on the given
        /// enumeration value, else will fall back to the implementation of <see cref="Enum.ToString()"/>.
        /// </summary>
        /// <param name="value">
        /// The value to get a description for.
        /// </param>
        /// <returns>
        /// The description of the given enumeration value.
        /// </returns>
        public static string GetDescription(this Enum value)
        {
            Guard.NotNull(nameof(value), value);

            var stringValue = value.ToString();
            var memberInfo = value.GetType().GetMember(stringValue);

            if (memberInfo.Length > 0)
            {
                var descriptionAttribute = memberInfo[0].GetAttributes<DescriptionAttribute>(false).FirstOrDefault();

                if (descriptionAttribute != null)
                {
                    return descriptionAttribute.Description;
                }
            }

            return stringValue;
        }
    }
}