using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;

namespace Blueprint.Utilities
{
    /// <summary>
    /// A small collection of utility methods when dealing with enums.
    /// </summary>
    [PublicAPI]
    public static class EnumUtilities
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

        /// <summary>
        /// For a given enum type will get all values that have been defined.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <returns>All defined values for the enumeration.</returns>
        public static IEnumerable<T> GetAllItems<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// For a given enum type will get the descriptions for all of the
        /// defined values.
        /// </summary>
        /// <param name="enumType">The type of the enumeration.</param>
        /// <returns>Descriptions for all defined values for the enumeration.</returns>
        /// <seealso cref="GetDescription"/>
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
        /// <seealso cref="GetDescription"/>
        public static Dictionary<string, string> GetDescriptionsWithKeys(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<Enum>().ToDictionary(e => e.ToString(), e => e.GetDescription());
        }

        /// <summary>
        /// Converts the specified string to an enumeration, matching on the enum name OR it's description if
        /// it has one (<see cref="DescriptionAttribute" />).
        /// </summary>
        /// <param name="description">The description to search for.</param>
        /// <param name="stringComparison">The type of string comparison to perform, defaulting to <see cref="StringComparison.OrdinalIgnoreCase"/>.</param>
        /// <typeparam name="T">The enumeration type.</typeparam>
        /// <returns>The value equivalent to the given description.</returns>
        /// <exception cref="ArgumentException">If an enum does not match.</exception>
        /// <seealso cref="TryFromDescription{T}(string,out T)" />
        public static T FromDescription<T>(string description, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) where T : Enum
        {
            if (TryFromDescription(description, stringComparison, out T found))
            {
                return found;
            }

            throw new ArgumentException($"Could not convert '{description}' to enum '{typeof(T).Name}");
        }

        /// <summary>
        /// Attempts to convert the specified string to an enumeration, matching on the enum name OR it's description if
        /// it has one (<see cref="DescriptionAttribute" />) using <see cref="StringComparison.OrdinalIgnoreCase" />.
        /// </summary>
        /// <param name="description">The description to search for.</param>
        /// <param name="value">The value equivalent to the given description.</param>
        /// <typeparam name="T">The enumeration type.</typeparam>
        /// <returns>Whether the conversion was successful.</returns>
        /// <exception cref="ArgumentException">If an enum does not match.</exception>
        /// <seealso cref="TryFromDescription{T}(string,out T)" />
        public static bool TryFromDescription<T>(string description, out T value) where T : Enum
        {
            return TryFromDescription(description, StringComparison.OrdinalIgnoreCase, out value);
        }

        /// <summary>
        /// Attempts to convert the specified string to an enumeration, matching on the enum name OR it's description if
        /// it has one (<see cref="DescriptionAttribute" />).
        /// </summary>
        /// <param name="description">The description to search for.</param>
        /// <param name="stringComparison">The type of string comparison to perform, defaulting to <see cref="StringComparison.OrdinalIgnoreCase"/>.</param>
        /// <param name="value">The value equivalent to the given description.</param>
        /// <typeparam name="T">The enumeration type.</typeparam>
        /// <returns>Whether the conversion was successful.</returns>
        /// <exception cref="ArgumentException">If an enum does not match.</exception>
        /// <seealso cref="TryFromDescription{T}(string,out T)" />]
        public static bool TryFromDescription<T>(string description, StringComparison stringComparison, out T value) where T : Enum
        {
            if (!string.IsNullOrEmpty(description))
            {
                foreach (var e in Enum.GetValues(typeof(T)).Cast<Enum>())
                {
                    if (e.ToString().Equals(description, stringComparison) || e.GetDescription().Equals(description, stringComparison))
                    {
                        value = (T)e;

                        return true;
                    }
                }
            }

            value = default;
            return false;
        }
    }
}
