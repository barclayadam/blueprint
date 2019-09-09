using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Blueprint.Api.Validation;
using Blueprint.Core.ThirdParty;
using Blueprint.Core.Utilities;
using Blueprint.Core.Validation;
using Newtonsoft.Json;

namespace Blueprint.Api
{
    /// <summary>
    /// An <see cref="IPropertyValidatorProvider"/> that generates the required property validator
    /// rules for client-side consumption.
    /// </summary>
    [UsedImplicitly]
    public class PropertyValidatorProvider : IPropertyValidatorProvider
    {
        private static readonly Dictionary<Type, PropertyValidator> TypeValidators = new Dictionary<Type, PropertyValidator>
        {
            { typeof(DateTime), new PropertyValidator { Name = "date", Parameter = "true" } },
            { typeof(int), new PropertyValidator { Name = "integer", Parameter = "true" } },
            { typeof(double), new PropertyValidator { Name = "numeric", Parameter = "true" } },
            { typeof(decimal), new PropertyValidator { Name = "numeric", Parameter = "true" } },
            { typeof(float), new PropertyValidator { Name = "numeric", Parameter = "true" } }
        };

        /// <summary>
        /// Gets all supported <see cref="PropertyValidator"/>s for the Blackout client side
        /// application framework.
        /// </summary>
        /// <param name="propertyInfo">The property to get validators for.</param>
        /// <returns>A (potentially empty) list of supported property validators for the given property.</returns>
        public IEnumerable<PropertyValidator> GetClientSideValidators(PropertyInfo propertyInfo)
        {
            foreach (var attribute in propertyInfo.GetCustomAttributes(true))
            {
                var handlerMethod = (from m in GetType().GetMethods(BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic)
                                     where m.GetParameters().Length == 2
                                     where m.GetParameters().ElementAt(0).ParameterType.IsInstanceOfType(attribute)
                                     where m.GetParameters().ElementAt(1).ParameterType == typeof(string)
                                     where m.ReturnType == typeof(PropertyValidator)
                                     select m).SingleOrDefault();

                var propertyName = propertyInfo.HasAttribute(typeof(DisplayNameAttribute), false)
                                    ? propertyInfo.GetAttributes<DisplayNameAttribute>(false).Single().DisplayName
                                    : propertyInfo.Name;

                if (handlerMethod != null)
                {
                    yield return (PropertyValidator)handlerMethod.Invoke(this, new[] { attribute, propertyName });
                }
            }

            if (TypeValidators.ContainsKey(propertyInfo.PropertyType.GetNonNullableType()))
            {
                yield return TypeValidators[propertyInfo.PropertyType.GetNonNullableType()];
            }
        }

        private static PropertyValidator Handle(RegexAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "regex",
                Parameter = "/{0}/{1}".Fmt(attribute.Regex.ToString(), attribute.Regex.Options.HasFlag(RegexOptions.IgnoreCase) ? "i" : string.Empty),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(RegularExpressionAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "regex",
                Parameter = "/{0}/".Fmt(attribute.Pattern),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(RangeAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "range",
                Parameter = "[{0},{1}]".Fmt(attribute.Minimum, attribute.Maximum),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(RequiredAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "required",
                Parameter = "true",
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(StringLengthAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "rangeLength",
                Parameter = "[{0},{1}]".Fmt(attribute.MinimumLength, attribute.MaximumLength),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(InFutureAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "inFuture",
                Parameter = "true",
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(InPastAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "inPast",
                Parameter = "true",
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(NotInFutureAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "notInFuture",
                Parameter = "true",
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(NotInPastAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "notInPast",
                Parameter = "true",
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(RequiredIfAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "requiredIf",
                Parameter = JsonConvert.SerializeObject(new
                {
                    property = attribute.DependentProperty.Camelize(),
                    equalsOneOf = attribute.DependentValues
                }),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(RequiredIfNotAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "requiredIfNot",
                Parameter = JsonConvert.SerializeObject(new
                {
                    property = attribute.DependantProperty.Camelize(),
                    equalsOneOf = attribute.DependantValues
                }),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(MustBeTrueAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "equalTo",
                Parameter = "{ value: true }",
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(GreaterThanAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "moreThan",
                Parameter = attribute.MinimumValue.ToString(),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(GreaterThanOrEqualToAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "min",
                Parameter = attribute.MinimumValue.ToString(),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(LessThanAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "lessThan",
                Parameter = attribute.MaximumValue.ToString(),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }

        private static PropertyValidator Handle(LessThanOrEqualToAttribute attribute, string propertyName)
        {
            return new PropertyValidator
            {
                Name = "max",
                Parameter = attribute.MaximumValue.ToString(),
                Message = attribute.FormatErrorMessage(propertyName)
            };
        }
    }
}
