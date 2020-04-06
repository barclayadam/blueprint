using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Blueprint.Api.Errors;
using Blueprint.Api.Middleware;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Core.Utilities;
using Microsoft.AspNetCore.Routing;

namespace Blueprint.Api.Http.MessagePopulation
{
    /// <summary>
    /// A <see cref="IMessagePopulationSource" /> that uses route data (from the URL the operation is
    /// accessed at).
    /// </summary>
    public class HttpRouteMessagePopulationSource : IMessagePopulationSource
    {
        /// <summary>
        /// Returns <c>100</c>.
        /// </summary>
        public int Priority => 100;

        // ReSharper disable once MemberCanBePrivate.Global Used in generated code
        public static object ConvertRouteValue(object routeValue, Type propertyType)
        {
            try
            {
                var typeConverter = TypeDescriptor.GetConverter(propertyType);

                return typeConverter.ConvertFrom(routeValue);
            }
            catch (Exception)
            {
                throw new NotFoundException($"Could not understand value '{routeValue}'");
            }
        }

        /// <summary>
        /// Returns the properties that are owned by this source, which are all the properties that
        /// come from routes (<see cref="LinkAttribute" />).
        /// </summary>
        /// <param name="apiDataModel">The API data model.</param>
        /// <param name="operationDescriptor">The descriptor to grab owned properties for.</param>
        /// <returns>The properties owned by this source, which are from links on the operation.</returns>
        public IEnumerable<PropertyInfo> GetOwnedProperties(ApiDataModel apiDataModel, ApiOperationDescriptor operationDescriptor)
        {
            var allLinks = apiDataModel.GetLinksForOperation(operationDescriptor.OperationType).ToList();

            // Grab all placeholder properties that are in ALL of the links for this operation. If it is in ALL links
            // we know we always "own" the property, otherwise we do not and therefore allow it to be filled by other
            // sources
            var placeholderProperties = allLinks
                .SelectMany(l => l.Placeholders)
                .Select(l => l.Property)
                .Where(routeProperty => allLinks.All(l => l.Placeholders.Any(ip => ip.Property == routeProperty)))
                .ToList();

            return placeholderProperties;
        }

        /// <inheritdoc />
        public void Build(IReadOnlyCollection<PropertyInfo> ownedProperties, MiddlewareBuilderContext context)
        {
            var allLinks = context.Model.GetLinksForOperation(context.Descriptor.OperationType).ToList();

            var placeholderProperties = allLinks
                .SelectMany(l => l.Placeholders)
                .ToList();

            var operationVariable = context.VariableFromContext(context.Descriptor.OperationType);

            // Add a single setter frame so we only grab using GetRouteData() once
            var routeDataFrame = new MethodCall(typeof(ApiOperationContextHttpExtensions), nameof(ApiOperationContextHttpExtensions.GetRouteData));
            context.ExecuteMethod.Frames.Add(routeDataFrame);

            foreach (var routePropertyPlaceholder in placeholderProperties)
            {
                var routeProperty = routePropertyPlaceholder.Property;
                var inAllRoutes = allLinks.All(l => l.Placeholders.Any(ip => ip.Property == routeProperty));

                if (routeProperty.PropertyType != typeof(string) && !TypeDescriptor.GetConverter(routeProperty.PropertyType).CanConvertFrom(typeof(string)))
                {
                    throw new InvalidOperationException(
                        $"Property {context.Descriptor.OperationType.Name}.{routeProperty.Name} cannot be used in routes, it cannot be converted from string");
                }

                var operationProperty = operationVariable.GetProperty(routeProperty.Name);
                var routeValuesVariable = routeDataFrame.ReturnVariable.GetProperty(nameof(RouteData.Values));


                // If the property exists in ALL routes for the operation then we know it _must_ exist in RouteData otherwise we would
                // not have got this far as the URL would not have matched. Therefore we do not need the TryGetValue method call and instead can
                // directly use the conversion expression from routeData[propertyName].
                if (inAllRoutes)
                {
                    var fromRouteData = $"{routeValuesVariable}[\"{routeProperty.Name}\"]";

                    // Generates "operation.[Property] = ([propertyType]) [expression];
                    // where [expression] can be a direct conversion, or
                    context.ExecuteMethod.Frames.Add(new VariableSetterFrame(
                        operationProperty,
                        GetConversionExpression(routeProperty, fromRouteData)));
                }
                else
                {
                    // When this property is not in ALL routes we use TryGetValue from the RouteData dictionary and pass
                    // the var output from that to `GetConversionExpression` as part of ternary. If the route data
                    // does not exist then we fallback to the value already on the operation
                    var outVariableName = "routeValue" + routeProperty.Name;
                    var conversionExpression = GetConversionExpression(routeProperty, outVariableName);

                    // context.RouteData.TryGetValue("PropertyName", out var routeValuePropertyName) ? [conversion of routeValuePropertyName] : [operationProperty];
                    var tryConversionExpression =
                        $"{routeValuesVariable}.{nameof(RouteValueDictionary.TryGetValue)}(\"{routeProperty.Name}\", out var {outVariableName}) ? " +
                        $"{conversionExpression} : " +
                        $"{operationProperty}";

                    context.ExecuteMethod.Frames.Add(new VariableSetterFrame(
                        operationProperty,
                        tryConversionExpression));
                }
            }
        }

        /// <summary>
        /// Gets the compile-time expression that will be used to convert the route data property to the required type
        /// for assignment to the operations' properties. For "simple" built-in types we will directly assign to avoid method
        /// call overhead, otherwise we delegate to
        /// </summary>
        /// <param name="routeProperty">The property that is to be set, used to determine it's type.</param>
        /// <param name="fromRouteData">A variable that is used to grab the data from route data.</param>
        /// <returns>An expression to be compiled-in that converts the given variable to the type of the property.</returns>
        private static string GetConversionExpression(PropertyInfo routeProperty, string fromRouteData)
        {
            var propertyType = routeProperty.PropertyType.GetNonNullableType();

            // No conversions needed if the type is a string
            if (propertyType == typeof(string))
            {
                return $"(string){fromRouteData}";
            }

            // A few hard-coded types that will be common in APIs are handled explicitly to avoid overhead of
            // using TypeDescriptor.GetConverter
            if (propertyType == typeof(Guid))
            {
                return $"{typeof(Guid).FullNameInCode()}.{nameof(Guid.Parse)}((string){fromRouteData})";
            }

            if (propertyType == typeof(int))
            {
                return $"{typeof(int).FullNameInCode()}.{nameof(int.Parse)}((string){fromRouteData})";
            }

            if (propertyType == typeof(long))
            {
                return $"{typeof(long).FullNameInCode()}.{nameof(long.Parse)}((string){fromRouteData})";
            }

            if (propertyType == typeof(double))
            {
                return $"{typeof(double).FullNameInCode()}.{nameof(double.Parse)}((string){fromRouteData})";
            }

            if (propertyType == typeof(short))
            {
                return $"{typeof(short).FullNameInCode()}.{nameof(short.Parse)}((string){fromRouteData})";
            }

            var methodCall = $"{typeof(HttpRouteMessagePopulationSource).FullNameInCode()}.{nameof(ConvertRouteValue)}";

            return $"({routeProperty.PropertyType.FullNameInCode()}) {methodCall}({fromRouteData}, typeof({routeProperty.PropertyType.FullNameInCode()}))";
        }
    }
}
