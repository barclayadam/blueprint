using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Core.Utilities
{
    /// <summary>
    /// A small collection of utility methods when dealing with enums.
    /// </summary>
    public static class EnumUtilities
    {
        /// <summary>
        /// For a given enum type will get all values that have been defined.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <returns>All defined values for the enumeration.</returns>
        public static IEnumerable<T> GetAllItems<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// For a given enum type will get the descriptions for all of the
        /// defined values.
        /// </summary>
        /// <param name="enumType">The type of the enumeration.</param>
        /// <returns>Descriptions for all defined values for the enumeration.</returns>
        /// <seealso cref="EnumExtensions.GetDescription"/>
        public static IEnumerable<string> GetDescriptions(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<Enum>().Select(e => e.GetDescription());
        }

        /// <summary>
        /// For a given enum type will get the descriptions for all of the
        /// defined values as a dictionary, with the keys being the ToString() representation
        /// of the enum.
        /// </summary>
        /// <param name="enumType">The type of the enumeration.</param>
        /// <returns>Descriptions for all defined values for the enumeration.</returns>
        /// <seealso cref="EnumExtensions.GetDescription"/>
        public static Dictionary<string, string> GetDescriptionsWithKeys(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<Enum>().ToDictionary(e => e.ToString(), e => e.GetDescription());
        }
    }
}