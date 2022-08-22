using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;
using Blueprint.Middleware;
using Blueprint.ThirdParty;
using Blueprint.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Blueprint.Http.MessagePopulation
{
    /// <summary>
    /// An <see cref="IMessagePopulationSource" /> that can load data from a variety of parts of
    /// a <see cref="HttpRequest" />, such as cookies, headers and query string values.
    /// </summary>
    /// <remarks>
    /// This source handles getting values from the "parts" it controls, in addition to potentially
    /// taking "ownership" of those properties if they are attributed to ensure they are not
    /// overriden by other sources and to optimise code generation by potentially eliminating
    /// some code paths if all properties are "owned".
    /// </remarks>
    public class HttpPartMessagePopulationSource : IMessagePopulationSource
    {
        private readonly bool _isCatchAll;
        private readonly Type _partAttribute;
        private readonly GetSourceVariable _sourceCodeExpression;
        private readonly bool _supportsMultiValues;
        private readonly Func<MiddlewareBuilderContext, bool> _applies;
        private readonly string _variablePrefix;

        private HttpPartMessagePopulationSource(Type partAttribute, GetSourceVariable sourceCodeExpression, bool supportsMultiValues)
        {
            this._partAttribute = partAttribute;
            this._sourceCodeExpression = sourceCodeExpression;
            this._supportsMultiValues = supportsMultiValues;
            this._isCatchAll = false;

            this._variablePrefix = partAttribute.Name.Replace("Attribute", string.Empty).Camelize();
        }

        private HttpPartMessagePopulationSource(
            string partName,
            GetSourceVariable sourceCodeExpression,
            Func<MiddlewareBuilderContext, bool> applies,
            bool supportsMultiValues)
        {
            this._sourceCodeExpression = sourceCodeExpression;
            this._supportsMultiValues = supportsMultiValues;
            this._applies = applies;
            this._isCatchAll = true;

            this._variablePrefix = partName;
        }

        /// <summary>
        /// Given a <see cref="Variable" /> that represents the <see cref="HttpContext" /> gets
        /// the <see cref="Variable" /> that is the source of this source (i.e. Cookies, Headers,
        /// Query)
        /// </summary>
        /// <param name="httpContextVariable">The HTTP context variable.</param>
        /// <returns>A variable representing the source for this message part.</returns>
        public delegate Variable GetSourceVariable(Variable httpContextVariable);

        /// <summary>
        /// Returns <c>0</c>.
        /// </summary>
        public int Priority => 0;

        /// <summary>
        /// Creates a new <see cref="HttpPartMessagePopulationSource" /> that will look for properties
        /// that have an applied attribute.
        /// </summary>
        /// <param name="sourceCodeExpression">Delegate to get the property to load data from.</param>
        /// <param name="supportsMultiValues">Indicates whether the source of this part supports array-like values (i.e. ASP.NET parses
        /// and creates a <see cref="StringValues" /> representation).</param>
        /// <typeparam name="T">The attribute that is searched for.</typeparam>
        /// <returns>A new <see cref="HttpPartMessagePopulationSource"/>.</returns>
        public static HttpPartMessagePopulationSource Owned<T>(GetSourceVariable sourceCodeExpression, bool supportsMultiValues)
        {
            return new HttpPartMessagePopulationSource(typeof(T), sourceCodeExpression, supportsMultiValues);
        }

        /// <summary>
        /// Creates a new <see cref="HttpPartMessagePopulationSource" /> that will try to populate all
        /// non-owned properties with data from a given source.
        /// </summary>
        /// <param name="partName">A name used to identify this "part", being part of variable names created
        /// and therefore should be unique.</param>
        /// <param name="sourceCodeExpression">Delegate to get the property to load data from.</param>
        /// <param name="applies">Indicates whether this catch-all applies. If <c>false</c> is returned then
        /// NO code will be generated.</param>
        /// <param name="supportsMultiValues">Indicates whether the source of this part supports array-like values (i.e. ASP.NET parses
        /// and creates a <see cref="StringValues" /> representation).</param>
        /// <returns>A new <see cref="HttpPartMessagePopulationSource"/>.</returns>
        public static HttpPartMessagePopulationSource CatchAll(
            string partName,
            GetSourceVariable sourceCodeExpression,
            Func<MiddlewareBuilderContext, bool> applies,
            bool supportsMultiValues)
        {
            return new HttpPartMessagePopulationSource(partName, sourceCodeExpression, applies, supportsMultiValues);
        }

        public static List<T> ConvertValuesToList<T>(StringValues values)
        {
            var list = new List<T>(values.Count);
            var converter = TypeDescriptor.GetConverter(typeof(T));

            foreach (var value in values)
            {
                list.Add((T)converter.ConvertFrom(value));
            }

            return list;
        }

        public static T[] ConvertValuesToArray<T>(StringValues values)
        {
            var array = new T[values.Count];
            var converter = TypeDescriptor.GetConverter(typeof(T));

            for (var i = 0; i < values.Count; i++)
            {
                array[i] = (T)converter.ConvertFrom(values[i]);
            }

            return array;
        }

        /// <summary>
        /// Gets the compile-time expression that will be used to convert the route data property to the required type
        /// for assignment to the operations' properties. For "simple" built-in types we will directly assign to avoid method
        /// call overhead, otherwise we delegate to
        /// </summary>
        /// <param name="property">The property that is to be set, used to determine it's type.</param>
        /// <param name="valueAccessor">A variable that is used to grab the data from route data.</param>
        /// <param name="supportsMultiValues">Whether the source supports multi-values (i.e. <see cref="StringValues" />).</param>
        /// <param name="sourceName">The name of the source, used in exceptions for unsupported property types.</param>
        /// <returns>An expression to be compiled-in that converts the given variable to the type of the property.</returns>
        public static string GetConversionExpression(
            PropertyInfo property,
            string valueAccessor,
            bool supportsMultiValues,
            string sourceName)
        {
            var propertyType = property.PropertyType.GetNonNullableType();

            // No conversions needed if the type is a string
            if (propertyType == typeof(string))
            {
                return $"{valueAccessor}.ToString()";
            }

            // A few hard-coded types that will be common in APIs are handled explicitly to avoid overhead of
            // using TypeDescriptor.GetConverter
            if (propertyType == typeof(Guid))
            {
                return $"{typeof(Guid).FullNameInCode()}.{nameof(Guid.Parse)}({valueAccessor}.ToString())";
            }

            if (propertyType == typeof(int))
            {
                return $"{typeof(int).FullNameInCode()}.{nameof(int.Parse)}({valueAccessor}.ToString())";
            }

            if (propertyType == typeof(long))
            {
                return $"{typeof(long).FullNameInCode()}.{nameof(long.Parse)}({valueAccessor}.ToString())";
            }

            if (propertyType == typeof(double))
            {
                return $"{typeof(double).FullNameInCode()}.{nameof(double.Parse)}({valueAccessor}.ToString())";
            }

            if (propertyType == typeof(short))
            {
                return $"{typeof(short).FullNameInCode()}.{nameof(short.Parse)}({valueAccessor}.ToString())";
            }

            var propertyFullName = property.PropertyType.FullNameInCode();

            // We do not have an array, let's use the TypeDescriptor directly in the code
            if (IsArrayLike(propertyType, out var arrayItemType) == false)
            {
                var typeConverter = TypeDescriptor.GetConverter(propertyType);

                if (typeConverter.CanConvertFrom(typeof(string)))
                {
                    return
                        $"({propertyFullName}) {typeof(TypeDescriptor).FullNameInCode()}.{nameof(TypeDescriptor.GetConverter)}(typeof({propertyType.FullNameInCode()})).{nameof(TypeConverter.ConvertFrom)}({valueAccessor}.ToString())";
                }

                throw new InvalidOperationException(
                    $@"Cannot create decoder for property {property.DeclaringType!.Name}.{property.Name} as it is not a handled primitive type and no TypeConverter exists.");
            }

            if (!supportsMultiValues)
            {
                throw new InvalidOperationException(
                    $@"Cannot create decoder for property {property.DeclaringType!.Name}.{property.Name} as it is array-like and {sourceName} does not support multiple values.");
            }

            // We will call either ConvertValuesToArray or ConvertValuesToList depending on the source property type. If it's of type
            // IEnumerable we will use ToArray
            var methodCall = $"{typeof(HttpPartMessagePopulationSource).FullNameInCode()}.ConvertValues";
            var conversionSuffix = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>) ? "ToList" : "ToArray";

            return
                $"({propertyFullName}) {methodCall}{conversionSuffix}<{arrayItemType.FullNameInCode()}>({valueAccessor})";
        }

        /// <summary>
        /// Returns any properties that have the attribute this source represents.
        /// </summary>
        /// <param name="apiDataModel">The API data model.</param>
        /// <param name="operationDescriptor">The descriptor to grab owned properties for.</param>
        /// <returns>All properties with a custom attribute of the type this source represents.</returns>
        public IEnumerable<OwnedPropertyDescriptor> GetOwnedProperties(ApiDataModel apiDataModel, ApiOperationDescriptor operationDescriptor)
        {
            return this._isCatchAll
                ? Enumerable.Empty<OwnedPropertyDescriptor>()
                : operationDescriptor.Properties
                    .Where(p => p.GetCustomAttributes(this._partAttribute).Any())
                    .Select(p => new OwnedPropertyDescriptor(p)
                    {
                        PropertyName = this.GetPartKey(p),
                    });
        }

        /// <inheritdoc />
        public void Build(
            IReadOnlyCollection<OwnedPropertyDescriptor> ownedProperties,
            IReadOnlyCollection<OwnedPropertyDescriptor> ownedBySource,
            MiddlewareBuilderContext context)
        {
            // Never apply if we are not in a HTTP-supported operation
            if (context.Descriptor.TryGetFeatureData<HttpOperationFeatureData>(out var _) == false)
            {
                return;
            }

            if (this._applies?.Invoke(context) == false)
            {
                return;
            }

            var operationVariable = context.FindVariable(context.Descriptor.OperationType);
            var httpContextVariable = context.FindVariable(typeof(HttpContext));
            var sourceVariable = this._sourceCodeExpression(httpContextVariable);

            foreach (var prop in this._isCatchAll ? context.Descriptor.Properties : ownedBySource.Select(s => s.Property))
            {
                if (prop.CanWrite == false)
                {
                    continue;
                }

                // If this is a catch-all instance we DO NOT want to generate any code for a property
                // that is owned by a different source.
                if (this._isCatchAll && ownedProperties.Any(p => p.Property == prop))
                {
                    continue;
                }

                var partKey = this.GetPartKey(prop);
                var operationProperty = operationVariable.GetProperty(prop.Name);

                // When this property is not in ALL routes we use TryGetValue from the RouteData dictionary and pass
                // the var output from that to `GetConversionExpression` as part of ternary. If the route data
                // does not exist then we fallback to the value already on the operation
                var outVariableName = $"{this._variablePrefix}{prop.Name}";
                var conversionExpression = GetConversionExpression(prop, outVariableName, this._supportsMultiValues, this._variablePrefix);

                var indexProperty = sourceVariable.VariableType.GetProperties().SingleOrDefault(p => p.GetIndexParameters().Length == 1);

                if (indexProperty == null)
                {
                    throw new InvalidOperationException($"Source variable {sourceVariable} has no index property specified. Cannot use");
                }

                var isStringValues = indexProperty.PropertyType == typeof(StringValues);
                var emptyCheckRhs = isStringValues ? $"{typeof(StringValues).FullNameInCode()}.Empty" : "null";

                var source = $"{sourceVariable}[\"{partKey}\"]";

                // We want to support the key naming scheme of key[] to indicate an array, therefore
                // we add a null-coalesce to partKey[]
                if (IsArrayLike(prop.PropertyType, out _))
                {
                    source = $"{source} == {emptyCheckRhs} ? {sourceVariable}[\"{partKey}[]\"] : {source}";
                }

                var sourceVariableGetter = new VariableCreationFrame(typeof(object), outVariableName, source);

                // [Source] == null ? [conversion of valuePropertyName] : [operationProperty];
                var tryConversionExpression =
                    $"{sourceVariableGetter.CreatedVariable} != {emptyCheckRhs} ? " +
                    $"{conversionExpression} : " +
                    $"{operationProperty}";

                context.AppendFrames(
                    sourceVariableGetter,
                    new VariableSetterFrame(
                        operationProperty,
                        tryConversionExpression),
                    new BlankLineFrame());
            }
        }

        private static bool IsArrayLike(Type type, out Type itemType)
        {
            var isList = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

            if (isList)
            {
                itemType = type.GetGenericArguments()[0];
                return true;
            }

            var isIEnumerable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

            if (isIEnumerable)
            {
                itemType = type.GetGenericArguments()[0];
                return true;
            }

            if (type.IsArray)
            {
                itemType = type.GetElementType();
                return true;
            }

            itemType = null;
            return false;
        }

        /// <summary>
        /// Given a <see cref="PropertyInfo" /> finds the key that will be used to load the data from the HTTP
        /// request for the specified property, done by trying to find the "part attribute" and grabbing
        /// the first constructor argument (that we know must be the override).
        /// </summary>
        /// <param name="prop">The prop to find the key for.</param>
        /// <returns>The key used for the part for this property.</returns>
        private string GetPartKey(PropertyInfo prop)
        {
            if (this._partAttribute == null)
            {
                return prop.Name;
            }

            var attribute = prop.GetCustomAttributeData(this._partAttribute);

            return attribute?.GetConstructorArgument<string>(0) ?? prop.Name;
        }
    }
}
