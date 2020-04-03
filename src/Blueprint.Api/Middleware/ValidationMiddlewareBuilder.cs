using System.Collections.Generic;
using System.Linq;
using Blueprint.Api.Validation;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// A middleware component that will validate the operation that is being executed.
    /// </summary>
    public class ValidationMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Returns <c>true</c>.
        /// </summary>
        public bool SupportsNestedExecution => true;

        /// <summary>
        /// Given a <see cref="System.ComponentModel.DataAnnotations.ValidationException"/> will convert it to an
        /// equivalent <see cref="ValidationFailedOperationResult" /> to be output to the user.
        /// </summary>
        /// <param name="validationException">The exception to be converted.</param>
        /// <returns>The response to send back to the client.</returns>
        // ReSharper disable once MemberCanBePrivate.Global This is used in generated code
        public static ValidationFailedOperationResult ToValidationFailedOperationResult(System.ComponentModel.DataAnnotations.ValidationException validationException)
        {
            var validationResult = validationException.ValidationResult;

            if (!validationResult.MemberNames.Any())
            {
                return new ValidationFailedOperationResult(validationResult.ErrorMessage);
            }

            var errorMessages = (IEnumerable<string>)new[] { validationResult.ErrorMessage };

            return new ValidationFailedOperationResult(validationResult.MemberNames.ToDictionary(m => m, m => errorMessages));
        }

        /// <summary>
        /// Always returns true as global exception handlers are added, although no actual validation code will
        /// be output in the case that no validation attributes exist on the operation.
        /// </summary>
        /// <param name="operation"><inheritdoc /></param>
        /// <returns><c>true</c> as we always need to handle unexpected validation exceptions.</returns>
        public bool Matches(ApiOperationDescriptor operation)
        {
            return true;
        }

        /// <inheritdoc />
        public void Build(MiddlewareBuilderContext context)
        {
            var properties = context.Descriptor.Properties;
            var operationVariable = context.VariableFromContext(context.Descriptor.OperationType);
            var apiOperationDescriptorVariable = context.VariableFromContext<ApiOperationDescriptor>();
            var resultsCreator = new ConstructorFrame<ValidationFailures>(() => new ValidationFailures());
            var hasValidationFrames = false;
            var sources = context.ServiceProvider.GetServices<IValidationSourceBuilder>();

            void AddValidatorFrame(Frame frame)
            {
                if (!hasValidationFrames)
                {
                    // Only add the "results creator" frame if there are any actual validation calls. This is the line
                    // that creates an empty ValidationFailures
                    context.ExecuteMethod.Frames.Add(resultsCreator);

                    hasValidationFrames = true;
                }

                context.ExecuteMethod.Frames.Add(frame);
            }

            var operationProperties = new List<OperationProperty>();

            for (var i = 0; i < properties.Length; i++)
            {
                var p = properties[i];

                var propertyInfoVariable = new PropertyInfoVariable(p, $"{apiOperationDescriptorVariable}.{nameof(ApiOperationDescriptor.Properties)}[{i}]");
                var propertyAttributesVariable = new Variable(typeof(object[]), $"{apiOperationDescriptorVariable}.{nameof(ApiOperationDescriptor.PropertyAttributes)}[{i}]");
                var propertyValueVariable = operationVariable.GetProperty(p.Name);

                operationProperties.Add(new OperationProperty
                {
                    PropertyInfoVariable = propertyInfoVariable,
                    PropertyValueVariable = propertyValueVariable,
                    PropertyAttributesVariable = propertyAttributesVariable,
                });
            }

            foreach (var s in sources)
            {
                foreach (var frame in s.GetFrames(operationVariable, operationProperties))
                {
                    AddValidatorFrame(frame);
                }
            }

            // Only bother trying to validate if properties actually exist that are validated
            if (hasValidationFrames)
            {
                /*
                 * var validationFailures = _from above_;
                 *
                 * if (validationFailures.Count > 0)
                 * {
                 *     var validationResult = new ValidationResult(validationFailures);
                 *
                 *     return validationResult;
                 * }
                 */
                var createResult = new ConstructorFrame<ValidationFailedOperationResult>(() => new ValidationFailedOperationResult((ValidationFailures)null));

                context.AppendFrames(
                    new IfBlock($"{resultsCreator.Variable}.{nameof(ValidationFailures.Count)} > 0")
                    {
                        createResult,
                        new ReturnFrame(createResult.Variable),
                    });
            }

            // Always need to register extra exception handlers because the operation handler itself may do additional validation
            // and throw an exception to indicate a problem, even if the operation itself is _not_ validated
            context.RegisterUnhandledExceptionHandler(typeof(ValidationException), RegisterBlueprintExceptionHandler);
            context.RegisterUnhandledExceptionHandler(typeof(System.ComponentModel.DataAnnotations.ValidationException), RegisterDataAnnotationsExceptionHandler);
        }

        private static IEnumerable<Frame> RegisterBlueprintExceptionHandler(Variable exception)
        {
            /*
             * var validationResult = new Blueprint.Core.Api.Middleware.ValidationResult(e.ValidationResults);
             * return validationResult;
             */
            var validationFailures = new Variable(typeof(ValidationFailures), $"{exception}.{nameof(ValidationException.ValidationResults)}");

            var createResult = new ConstructorFrame<ValidationFailedOperationResult>(() => new ValidationFailedOperationResult((ValidationFailures)null))
            {
                Parameters = { [0] = validationFailures },
            };

            yield return createResult;
            yield return new ReturnFrame(createResult.Variable);
        }

        private static IEnumerable<Frame> RegisterDataAnnotationsExceptionHandler(Variable exception)
        {
            /*
             * var validationResult = Blueprint.Core.Api.Middleware.ValidationMiddlewareBuilder.ToValidationFailedOperationResult(e);
             * return validationResult;
             */
            var createResponse = new MethodCall(typeof(ValidationMiddlewareBuilder), nameof(ToValidationFailedOperationResult))
            {
                Arguments = { [0] = exception },
            };

            yield return createResponse;
            yield return new ReturnFrame(createResponse.ReturnVariable);
        }
    }
}
