using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Blueprint.Compiler.Frames;
using Blueprint.Middleware;
using Microsoft.AspNetCore.Routing;

namespace Blueprint.Http.MessagePopulation
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

        /// <summary>
        /// Returns the properties that are owned by this source, which are all the properties that
        /// come from routes (<see cref="LinkAttribute" />).
        /// </summary>
        /// <param name="apiDataModel">The API data model.</param>
        /// <param name="operationDescriptor">The descriptor to grab owned properties for.</param>
        /// <returns>The properties owned by this source, which are from links on the operation.</returns>
        public IEnumerable<OwnedPropertyDescriptor> GetOwnedProperties(ApiDataModel apiDataModel, ApiOperationDescriptor operationDescriptor)
        {
            var allLinks = apiDataModel.GetLinksForOperation(operationDescriptor.OperationType).ToList();

            // Grab all placeholder properties that are in ALL of the links for this operation. If it is in ALL links
            // we know we always "own" the property, otherwise we do not and therefore allow it to be filled by other
            // sources
            var placeholderProperties = allLinks
                .SelectMany(l => l.Placeholders)
                .Select(l => l.Property)
                .Where(routeProperty => allLinks.All(l => l.Placeholders.Any(ip => ip.Property == routeProperty)))
                .Select(p => new OwnedPropertyDescriptor(p))
                .ToList();

            return placeholderProperties;
        }

        /// <inheritdoc />
        public void Build(IReadOnlyCollection<OwnedPropertyDescriptor> ownedProperties, IReadOnlyCollection<OwnedPropertyDescriptor> ownedBySource,
            MiddlewareBuilderContext context)
        {
            var allLinks = context.Model.GetLinksForOperation(context.Descriptor.OperationType).ToList();

            var placeholderProperties = allLinks
                .SelectMany(l => l.Placeholders)
                .ToList();

            var operationVariable = context.FindVariable(context.Descriptor.OperationType);

            // Add a single setter frame so we only grab using GetRouteData() once
            var routeDataFrame = new MethodCall(typeof(ApiOperationContextHttpExtensions), nameof(ApiOperationContextHttpExtensions.GetRouteData));

            if (placeholderProperties.Any())
            {
                context.ExecuteMethod.Frames.Add(routeDataFrame);
            }

            foreach (var routePropertyPlaceholder in placeholderProperties)
            {
                var routeProperty = routePropertyPlaceholder.Property;

                if (routeProperty.PropertyType != typeof(string) && !TypeDescriptor.GetConverter(routeProperty.PropertyType).CanConvertFrom(typeof(string)))
                {
                    throw new InvalidOperationException(
                        $"Property {context.Descriptor.OperationType.Name}.{routeProperty.Name} cannot be used in routes, it cannot be converted from string");
                }

                var operationProperty = operationVariable.GetProperty(routeProperty.Name);
                var routeValuesVariable = routeDataFrame.ReturnVariable.GetProperty(nameof(RouteData.Values));

                // When this property is not in ALL routes we use TryGetValue from the RouteData dictionary and pass
                // the var output from that to `GetConversionExpression` as part of ternary. If the route data
                // does not exist then we fallback to the value already on the operation
                var outVariableName = "routeValue" + routeProperty.Name;
                var conversionExpression = HttpPartMessagePopulationSource.GetConversionExpression(routeProperty, outVariableName);

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
}
