using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using NJsonSchema;
using NJsonSchema.Generation;

namespace Blueprint.OpenApi
{
    /// <summary>
    /// Custom <see cref="JsonSchemaGenerator"/> that overrides the generation of enums to only use the string generation
    /// to work around a bug that generates duplicate enum values (e.g. <see cref="HttpStatusCode"/>).
    /// https://github.com/RicoSuter/NJsonSchema/issues/800
    /// </summary>
    public class BlueprintJsonSchemaGenerator : JsonSchemaGenerator
    {
        /// <summary>
        /// Initialise a default instance of the <see cref="BlueprintJsonSchemaGenerator"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public BlueprintJsonSchemaGenerator(JsonSchemaGeneratorSettings settings) : base(settings)
        {
        }

        /// <summary>Generates an enumeration in the given schema.</summary>
        /// <param name="schema">The schema.</param>
        /// <param name="typeDescription">The type description.</param>
        protected override void GenerateEnum(JsonSchema schema, JsonTypeDescription typeDescription)
        {
            var contextualType = typeDescription.ContextualType;

            schema.Type = typeDescription.Type;
            schema.Enumeration.Clear();
            schema.EnumerationNames.Clear();
            schema.IsFlagEnumerable = contextualType.GetTypeAttribute<FlagsAttribute>() != null;

            var underlyingType = Enum.GetUnderlyingType(contextualType.Type);

            foreach (var enumName in Enum.GetNames(contextualType.Type))
            {
                if (typeDescription.Type == JsonObjectType.Integer)
                {
                    var value = Convert.ChangeType(Enum.Parse(contextualType.Type, enumName), underlyingType);
                    schema.Enumeration.Add(value);
                }
                else
                {
                    // EnumMember only checked if StringEnumConverter is used
                    var attributes = contextualType.TypeInfo.GetDeclaredField(enumName).GetCustomAttributes(typeof(EnumMemberAttribute));
                    var enumMemberAttribute = (EnumMemberAttribute)attributes.FirstOrDefault();
                    if (enumMemberAttribute != null && !string.IsNullOrEmpty(enumMemberAttribute.Value))
                    {
                        schema.Enumeration.Add(enumMemberAttribute.Value);
                    }
                    else
                    {
                        schema.Enumeration.Add(enumName);
                    }
                }

                schema.EnumerationNames.Add(enumName);
            }

            if (typeDescription.Type == JsonObjectType.Integer && Settings.GenerateEnumMappingDescription)
            {
                schema.Description = (schema.Description + "\n\n" +
                    string.Join("\n", schema.Enumeration.Select((e, i) => e + " = " + schema.EnumerationNames[i]))).Trim();
            }
        }
    }
}
