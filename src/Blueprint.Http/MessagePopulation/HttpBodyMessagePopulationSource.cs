using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.Compiler;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;
using Blueprint.Http.Formatters;
using Blueprint.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blueprint.Http.MessagePopulation;

/// <summary>
/// A <see cref="IMessagePopulationSource" /> that will read data from a HTTP message body.
/// </summary>
public class HttpBodyMessagePopulationSource : IMessagePopulationSource
{
    private static readonly MethodInfo _populateBodyMethod = typeof(HttpBodyMessagePopulationSource).GetMethod(nameof(PopulateFromMessageBody))!;

    /// <summary>
    /// Returns <c>0</c>.
    /// </summary>
    public int Priority => 0;

    /// <summary>
    /// Supporting method that will attempt to populate the given operation from the body of
    /// a HTTP request by searching for an applicable <see cref="IBodyParser" /> that has been
    /// specified in the <see cref="BlueprintHttpOptions.BodyParsers" /> property.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="context">The operation context.</param>
    /// <param name="log">A log to write to.</param>
    /// <param name="options">The registered HTTP options.</param>
    /// <param name="instance">The instance to populate.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    /// <typeparam name="T">The type of operation being populated.</typeparam>
    // ReSharper disable once MemberCanBePrivate.Global Used in generated code
    public static async Task<T> PopulateFromMessageBody<T>(
        HttpContext httpContext,
        ApiOperationContext context,
        ILogger<HttpBodyMessagePopulationSource> log,
        IOptions<BlueprintHttpOptions> options,
        T instance)
    {
        var type = typeof(T);
        var request = httpContext.Request;

        if (request.Body == null || request.ContentType == null)
        {
            return instance;
        }

        log?.AttemptingToParseBody(type);

        var formatterContext = new BodyParserContext(
            context,
            httpContext,
            instance,
            type);

        var parsers = options.Value.BodyParsers;
        var formatter = (IBodyParser)null;

        for (var i = 0; i < parsers.Count; i++)
        {
            if (parsers[i].CanRead(formatterContext))
            {
                formatter = parsers[i];
                log.BodyParserSelected(formatter, formatterContext);
                break;
            }

            log.BodyParserRejected(parsers[i], formatterContext);
        }

        if (formatter == null)
        {
            // TODO: How to handle no body parsers matching?
            log.NoBodyParserSelected(formatterContext);

            return instance;
        }

        // TODO: Handle exceptions
        try
        {
            return (T)await formatter.ReadAsync(formatterContext);
        }
        catch (Exception e)
        {
            log.BodyParsingException(type, e);

            throw;
        }
        finally
        {
            log.DoneParsingBody(type, formatter);
        }
    }

    /// <summary>
    /// Returns an empty enumeration, this is a "catch-all" type of source.
    /// </summary>
    /// <param name="apiDataModel">The API data model.</param>
    /// <param name="operationDescriptor">The descriptor to grab owned properties for.</param>
    /// <returns>An empty enumeration.</returns>
    public IEnumerable<OwnedPropertyDescriptor> GetOwnedProperties(ApiDataModel apiDataModel, ApiOperationDescriptor operationDescriptor)
    {
        return operationDescriptor.Properties
            .Where(p => p.GetCustomAttributes(typeof(FromBodyAttribute), false).Any())
            .Select(p => new OwnedPropertyDescriptor(p)
            {
                PropertyName = p.Name,
            });
    }

    /// <inheritdoc />
    public void Build(
        IReadOnlyCollection<OwnedPropertyDescriptor> ownedProperties,
        IReadOnlyCollection<OwnedPropertyDescriptor> ownedBySource,
        MiddlewareBuilderContext context)
    {
        // We can bail early on any code generation as we know that all properties are fulfilled by
        // other sources
        if (ownedProperties.Count == context.Descriptor.Properties.Length && ownedBySource.Count == 0)
        {
            return;
        }

        if (context.Descriptor.TryGetFeatureData<HttpOperationFeatureData>(out var httpData) == false)
        {
            return;
        }

        if (ownedBySource.Count > 1)
        {
            throw new InvalidOperationException(
                $"Operation {context.Descriptor.OperationType} declares multiple properties with the {nameof(FromBodyAttribute)} which is not allowed. At most one property can be decorated with that attribute.");
        }

        // If the request method is a GET then there will be no body, and therefore we do not need to attempt to
        // read the message body at all.
        if (httpData.HttpMethod != "GET")
        {
            var operationVariable = context.FindVariable(context.Descriptor.OperationType);

            if (ownedBySource.Any())
            {
                // We have [FromBody] so that needs to be set, instead of populating the context.Operation property.
                var prop = ownedBySource.Single();
                var operationProperty = operationVariable.GetProperty(prop.Property.Name);

                var readCall = new MethodCall(
                    typeof(HttpBodyMessagePopulationSource),
                    _populateBodyMethod.MakeGenericMethod(operationProperty.VariableType));

                readCall.TrySetArgument(new Variable(operationProperty.VariableType, $"new {operationProperty.VariableType.FullNameInCode()}()"));

                context.AppendFrames(
                    readCall,
                    new VariableSetterFrame(operationProperty, readCall.ReturnVariable));
            }
            else
            {
                var readCall = new MethodCall(
                    typeof(HttpBodyMessagePopulationSource),
                    _populateBodyMethod.MakeGenericMethod(context.Descriptor.OperationType));

                readCall.TrySetArgument(operationVariable);
                readCall.ReturnVariable.OverrideName("parseBodyResult");

                var setInOperation = new VariableSetterFrame(operationVariable, readCall.ReturnVariable);
                var setAmbient =
                    new VariableSetterFrame(
                        context.FindVariable(typeof(ApiOperationContext)).GetProperty(nameof(ApiOperationContext.Operation)),
                        readCall.ReturnVariable);

                context.AppendFrames(
                    readCall,
                    setInOperation,
                    setAmbient);
            }
        }
    }
}