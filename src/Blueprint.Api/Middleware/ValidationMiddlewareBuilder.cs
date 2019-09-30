using System.Collections.Generic;
using System.Linq;
using Blueprint.Api.Validation;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Core.Errors;
using Microsoft.Extensions.DependencyInjection;
using ValidationErrorResponse = Blueprint.Api.Validation.ValidationErrorResponse;
using ValidationException = Blueprint.Api.Validation.ValidationException;

namespace Blueprint.Api.Middleware
{
    /// <summary>
    /// A middleware component that will validate the operation that is being executed.
    /// </summary>
    public class ValidationMiddlewareBuilder : IMiddlewareBuilder
    {
        /// <summary>
        /// Given a <see cref="System.ComponentModel.DataAnnotations.ValidationException"/> will convert it to an
        /// equivalent <see cref="ValidationErrorResponse" /> to be output to the user.
        /// </summary>
        /// <param name="validationException">The exception to be converted.</param>
        /// <returns>The response to send back to the client.</returns>
        // ReSharper disable once MemberCanBePrivate.Global This is used in generated code
        public static ValidationErrorResponse ToErrorResponse(System.ComponentModel.DataAnnotations.ValidationException validationException)
        {
            var validationResult = validationException.ValidationResult;

            if (!validationResult.MemberNames.Any())
            {
                return new ValidationErrorResponse(new Dictionary<string, IEnumerable<string>>
                {
                    [ValidationFailures.FormLevelPropertyName] = new[] { validationResult.ErrorMessage },
                });
            }

            var errorMessages = (IEnumerable<string>)new[] { validationResult.ErrorMessage };

            return new ValidationErrorResponse(validationResult.MemberNames.ToDictionary(m => m, m => errorMessages));
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
                    // Only add this frame if there are any actual validation calls
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
                 *     var validationErrorResponse = new ValidationErrorResponse(validationFailures);
                 *     var validationResult = new ValidationResult(validationErrorResponse);
                 *
                 *     return validationResult;
                 * }
                 */
                var createResponse = new ConstructorFrame<ValidationErrorResponse>(() => new ValidationErrorResponse((ValidationFailures)null));
                var createResult = new ConstructorFrame<ValidationFailedResult>(() => new ValidationFailedResult((ValidationErrorResponse)null));

                context.AppendFrames(
                    new IfBlock(
                        $"{resultsCreator.Variable}.{nameof(ValidationFailures.Count)} > 0",
                        createResponse,
                        createResult,
                        new ReturnFrame(createResult.Variable)));
            }

            // Always need to register extra exception handlers because the operation handler itself may do additional validation
            // and throw an exception to indicate a problem, even if the operation itself is _not_ validated
            context.RegisterUnhandledExceptionHandler(typeof(ValidationException), RegisterBlueprintExceptionHandler);
            context.RegisterUnhandledExceptionHandler(typeof(System.ComponentModel.DataAnnotations.ValidationException), RegisterDataAnnotationsExceptionHandler);
        }

        private static IEnumerable<Frame> RegisterBlueprintExceptionHandler(Variable exception)
        {
            /*
             * var validationErrorResponse = new Blueprint.Core.Errors.ValidationErrorResponse(e.ValidationResults);
             * var validationResult = new Blueprint.Core.Api.Middleware.ValidationResult(validationErrorResponse);
             * return validationResult;
             */
            var validationFailures = new Variable(typeof(ValidationFailures), $"{exception}.{nameof(ValidationException.ValidationResults)}");
            var createResponse =
                new ConstructorFrame<ValidationErrorResponse>(() =>
                    new ValidationErrorResponse((ValidationFailures)null))
                {
                    Parameters = { [0] = validationFailures },
                };

            yield return createResponse;
            yield return new ConstructorFrame<ValidationFailedResult>(() => new ValidationFailedResult((ValidationErrorResponse)null));
            yield return new ReturnFrame(typeof(ValidationFailedResult));
        }

        private static IEnumerable<Frame> RegisterDataAnnotationsExceptionHandler(Variable exception)
        {
            /*
             * var validationErrorResponse = Blueprint.Core.Api.Middleware.ValidationMiddlewareBuilder.ToErrorResponse(e);
             * var validationResult = new Blueprint.Core.Api.Middleware.ValidationResult(validationErrorResponse);
             * return validationResult;
             */
            yield return new MethodCall(typeof(ValidationMiddlewareBuilder), nameof(ToErrorResponse))
            {
                Arguments = { [0] = exception },
            };
            yield return new ConstructorFrame<ValidationFailedResult>(() => new ValidationFailedResult((ValidationErrorResponse)null));
            yield return new ReturnFrame(typeof(ValidationFailedResult));
        }
    }
}
