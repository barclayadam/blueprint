using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Blueprint.Auditing;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Middleware;

/// <summary>
/// A middleware that will populate the API operation using all registered
/// <see cref="IMessagePopulationSource" />s.
/// </summary>
public class MessagePopulationMiddlewareBuilder : IMiddlewareBuilder
{
    /// <summary>
    /// Returns <c>false</c>.
    /// </summary>
    public bool SupportsNestedExecution => false;

    /// <summary>
    /// Returns <c>true</c> if <paramref name="operation"/>.<see cref="ApiOperationDescriptor.OperationType"/> has any
    /// properties, <c>false</c> otherwise (as no properties == nothing to set).
    /// </summary>
    /// <param name="operation">The operation to check against.</param>
    /// <returns>Whether to apply this middleware.</returns>
    public bool Matches(ApiOperationDescriptor operation)
    {
        return operation.Properties.Any();
    }

    /// <inheritdoc />
    public void Build(MiddlewareBuilderContext context)
    {
        var sources = context.ServiceProvider
            .GetServices<IMessagePopulationSource>()
            .OrderBy(s => s.Priority)
            .ToList();

        var allOwned = new List<OwnedPropertyDescriptor>();
        var ownedBySources = new Dictionary<IMessagePopulationSource, IReadOnlyCollection<OwnedPropertyDescriptor>>();

        foreach (var s in sources)
        {
            var ownedBySource = s.GetOwnedProperties(context.Model, context.Descriptor).ToList();

            ownedBySources[s] = ownedBySource;

            foreach (var p in ownedBySource)
            {
                if (allOwned.Contains(p))
                {
                    throw new InvalidOperationException(
                        $"Property {p.Property.DeclaringType.Name}.{p.Property.Name} has been marked as owned by multiple sources. A property can only be owned by a single source.");
                }

                allOwned.Add(p);
            }
        }

        foreach (var s in sources)
        {
            s.Build(allOwned, ownedBySources[s], context);
        }

        context.AppendFrames(new SetTagsFromMessageDataFrame(context));
    }

    /// <summary>
    /// A frame that will, for every non-sensitive property, add a tag to the current Activity.
    /// </summary>
    private class SetTagsFromMessageDataFrame : SyncFrame
    {
        private readonly MiddlewareBuilderContext _context;

        /// <summary>
        /// Initialises a new instance of the <see cref="SetTagsFromMessageDataFrame" /> class.
        /// </summary>
        /// <param name="context">The builder context this frame belongs to.</param>
        public SetTagsFromMessageDataFrame(MiddlewareBuilderContext context)
        {
            this._context = context;
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var apiOperationContextVariable = variables.FindVariable(typeof(ApiOperationContext));
            var activityVariable = apiOperationContextVariable.GetProperty(nameof(ApiOperationContext.Activity));

            var operationTypeKey = ReflectionUtilities.PrettyTypeName(this._context.Descriptor.OperationType);

            writer.BlankLine();

            // 2. For every property of the operation output a value to the exception.Data dictionary. All properties that are
            // not considered sensitive
            foreach (var prop in this._context.Descriptor.Properties)
            {
                if (SensitiveProperties.IsSensitive(prop))
                {
                    continue;
                }

                // Only support, for now, primitive values, strings and GUIDs to avoid pushing complex types as tags
                if (!prop.PropertyType.IsPrimitive &&
                    !prop.PropertyType.IsEnum &&
                    prop.PropertyType.GetNonNullableType() != typeof(string) &&
                    prop.PropertyType.GetNonNullableType() != typeof(Guid))
                {
                    continue;
                }

                writer.WriteLine(
                    $"{activityVariable}?.{nameof(Activity.SetTag)}(\"{operationTypeKey}.{prop.Name}\", {variables.FindVariable(this._context.Descriptor.OperationType)}.{prop.Name});");
            }

            next();
        }
    }
}