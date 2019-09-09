using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Notifications.Templates
{
    /// <summary>
    /// A parser which can take an IDictionary{string, object} and construct a new
    /// dictionary that is used within a TemplateValues instance, converting known
    /// values such as 'true' and 'false' into their real CLR types for better
    /// support within the templates.
    /// </summary>
    internal static class TemplateValuesParser
    {
        /// <summary>
        /// Parses the values in the given dictionary, creating a copy of the
        /// dictionary with the values parsed into correct types for use within
        /// the templating engines where possible.
        /// </summary>
        /// <remarks>
        /// This method will perform the following actions on the values:
        /// <list type="bullet">
        /// <item>Remove keys with values equal to <c>null</c> or an empty string</item>
        /// <item>Convert the string 'true' into a boolean type equal to <c>true</c></item>
        /// <item>Convert the string 'false' into a boolean type equal to <c>false</c></item>
        /// </list>.
        /// </remarks>
        /// <param name="inputValues">The input values to parse.</param>
        /// <returns>A new dictionary with values copied and parsed from the input values.</returns>
        public static IDictionary<string, object> Parse(IDictionary<string, object> inputValues)
        {
            if (inputValues == null || inputValues.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            var templateValues = new Dictionary<string, object>();

            foreach (var kvp in inputValues.Where(ShouldIncludeValue))
            {
                templateValues[kvp.Key] = GetConvertedValue(kvp.Value);
            }

            return templateValues;
        }

        private static object GetConvertedValue(object value)
        {
            if (string.Equals(value.ToString(), "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(value.ToString(), "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return value;
        }

        private static bool ShouldIncludeValue(KeyValuePair<string, object> kvp)
        {
            return kvp.Value != null && !string.IsNullOrEmpty(kvp.Value.ToString());
        }
    }
}