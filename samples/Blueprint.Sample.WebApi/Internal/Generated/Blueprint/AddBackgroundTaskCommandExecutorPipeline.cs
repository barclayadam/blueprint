// <auto-generated />
// AddBackgroundTaskCommandExecutorPipeline

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Blueprint.Generated
{
    public class AddBackgroundTaskCommandExecutorPipeline : Blueprint.IOperationExecutorPipeline
    {
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, System.Exception> _ValidationFailed = Microsoft.Extensions.Logging.LoggerMessage.Define<System.Int32>(Microsoft.Extensions.Logging.LogLevel.Debug, new Microsoft.Extensions.Logging.EventId(1, "ValidationFailed"), "Validation failed with {ValidationFailureCount} failures, returning ValidationFailedOperationResult");
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, string, System.Exception> _OperationExecuting = Microsoft.Extensions.Logging.LoggerMessage.Define<System.String>(Microsoft.Extensions.Logging.LogLevel.Debug, new Microsoft.Extensions.Logging.EventId(3, "OperationExecuting"), "Executing API operation {OperationType} with inline handler");
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, string, System.Exception> _AnonxxApiExceptionhasoccurredwithmessageExceptionMessage = Microsoft.Extensions.Logging.LoggerMessage.Define<System.String>(Microsoft.Extensions.Logging.LogLevel.Information, new Microsoft.Extensions.Logging.EventId(9, "A non-5xx ApiException has occurred with message {ExceptionMessage}"), "A non-5xx ApiException has occurred with message {ExceptionMessage}");
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, string, System.Exception> _AnunhandledexceptionhasoccurredwithmessageExceptionMessage = Microsoft.Extensions.Logging.LoggerMessage.Define<System.String>(Microsoft.Extensions.Logging.LogLevel.Error, new Microsoft.Extensions.Logging.EventId(10, "An unhandled exception has occurred with message {ExceptionMessage}"), "An unhandled exception has occurred with message {ExceptionMessage}");

        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly Microsoft.Extensions.Logging.ILogger<Blueprint.Http.MessagePopulation.HttpBodyMessagePopulationSource> _httpBodyMessagePopulationSourceILogger;
        private readonly Microsoft.Extensions.Options.IOptions<Blueprint.Http.BlueprintHttpOptions> _blueprintHttpOptionsIOptions;

        public AddBackgroundTaskCommandExecutorPipeline(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Microsoft.Extensions.Logging.ILogger<Blueprint.Http.MessagePopulation.HttpBodyMessagePopulationSource> httpBodyMessagePopulationSourceILogger, Microsoft.Extensions.Options.IOptions<Blueprint.Http.BlueprintHttpOptions> blueprintHttpOptionsIOptions)
        {
            _logger = loggerFactory.CreateLogger("AddBackgroundTaskCommandExecutorPipeline");
            _httpBodyMessagePopulationSourceILogger = httpBodyMessagePopulationSourceILogger;
            _blueprintHttpOptionsIOptions = blueprintHttpOptionsIOptions;
        }

        public async System.Threading.Tasks.Task<Blueprint.OperationResult> ExecuteAsync(Blueprint.ApiOperationContext context)
        {
            using var activityOfAddBackgroundTaskPipeline = Blueprint.Diagnostics.BlueprintActivitySource.ActivitySource.StartActivity("AddBackgroundTaskPipeline", System.Diagnostics.ActivityKind.Internal);
            var addBackgroundTaskCommand = (Blueprint.Sample.WebApi.Api.AddBackgroundTaskCommand) context.Operation;
            try
            {

                // MessagePopulationMiddlewareBuilder
                var httpContext = Blueprint.Http.ApiOperationContextHttpExtensions.GetHttpContext(context);
                var parseBodyResult = await Blueprint.Http.MessagePopulation.HttpBodyMessagePopulationSource.PopulateFromMessageBody<Blueprint.Sample.WebApi.Api.AddBackgroundTaskCommand>(httpContext, context, _httpBodyMessagePopulationSourceILogger, _blueprintHttpOptionsIOptions, addBackgroundTaskCommand);
                addBackgroundTaskCommand = parseBodyResult;
                context.Operation = parseBodyResult;

                // UserContextLoaderMiddlewareBuilder
                context.UserAuthorisationContext = Blueprint.AnonymousUserAuthorisationContext.Instance;

                // ValidationMiddlewareBuilder
                var validationFailures = new Blueprint.Validation.ValidationFailures();
                var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(addBackgroundTaskCommand);
                validationContext.MemberName = "Parameter";
                validationContext.DisplayName = "Parameter";

                // context.Descriptor.Properties[0] == AddBackgroundTaskCommand.Parameter
                foreach (var attribute in context.Descriptor.PropertyAttributes[0])
                {
                    if (attribute is System.ComponentModel.DataAnnotations.ValidationAttribute x)
                    {
                        var result =  x.GetValidationResult(addBackgroundTaskCommand.Parameter, validationContext);
                        if (result != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                        {
                            validationFailures.AddFailure(result);
                        }
                    }
                }
                if (validationFailures.Count > 0)
                {
                    _ValidationFailed(_logger, validationFailures.Count, null);
                    var validationFailedOperationResult = new Blueprint.Middleware.ValidationFailedOperationResult(validationFailures);
                    return validationFailedOperationResult;
                }

                // OperationExecutorMiddlewareBuilder
                using var activityOfAddBackgroundTaskCommand = Blueprint.Diagnostics.BlueprintActivitySource.ActivitySource.StartActivity("AddBackgroundTaskCommand", System.Diagnostics.ActivityKind.Internal);
                _OperationExecuting(_logger, "AddBackgroundTaskCommand", null);
                var backgroundTaskScheduler = context.ServiceProvider.GetRequiredService<Blueprint.Tasks.IBackgroundTaskScheduler>();
                var handlerResult = addBackgroundTaskCommand.Invoke(backgroundTaskScheduler);
                Blueprint.OperationResult operationResult = handlerResult;
                activityOfAddBackgroundTaskCommand?.Dispose();

                // BackgroundTaskRunnerMiddleware
                await backgroundTaskScheduler.RunNowAsync();

                // ResourceEventHandlerMiddlewareBuilder
                var resourceEventRepository = context.ServiceProvider.GetRequiredService<Blueprint.Http.IResourceEventRepository>();
                var apiLinkGenerator = context.ServiceProvider.GetRequiredService<Blueprint.Http.IApiLinkGenerator>();
                await Blueprint.Http.Middleware.ResourceEventHandler.HandleAsync(resourceEventRepository, apiLinkGenerator, context, operationResult);

                // ReturnFrameMiddlewareBuilder
                return operationResult;
            }
            catch (Blueprint.Validation.ValidationException e)
            {
                context.Activity?.SetTag("otel.status_code", "OK");
                var validationFailedOperationResult = new Blueprint.Middleware.ValidationFailedOperationResult(e.ValidationResults);
                return validationFailedOperationResult;
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException e)
            {
                context.Activity?.SetTag("otel.status_code", "OK");
                var validationFailedOperationResult = Blueprint.Middleware.ValidationMiddlewareBuilder.ToValidationFailedOperationResult(e);
                return validationFailedOperationResult;
            }
            catch (System.Exception e)
            {
                if (e is Blueprint.ApiException apiException && apiException.HttpStatus < 500)
                {
                    _AnonxxApiExceptionhasoccurredwithmessageExceptionMessage(_logger, e.Message, e);
                    context.Activity?.SetTag("otel.status_code", "OK");
                }
                else
                {
                    Blueprint.Diagnostics.BlueprintActivitySource.RecordException(context.Activity, e, false);
                    _AnunhandledexceptionhasoccurredwithmessageExceptionMessage(_logger, e.Message, e);
                    context.Activity?.SetTag("otel.status_code", "ERROR");
                    context.Activity?.SetTag("otel.status_description", e.Message);
                }
                return new Blueprint.UnhandledExceptionOperationResult(e);
            }
        }

        public async System.Threading.Tasks.Task<Blueprint.OperationResult> ExecuteNestedAsync(Blueprint.ApiOperationContext context)
        {
            using var activityOfAddBackgroundTaskNestedPipeline = Blueprint.Diagnostics.BlueprintActivitySource.ActivitySource.StartActivity("AddBackgroundTaskNestedPipeline", System.Diagnostics.ActivityKind.Internal);
            var addBackgroundTaskCommand = (Blueprint.Sample.WebApi.Api.AddBackgroundTaskCommand) context.Operation;
            try
            {

                // ValidationMiddlewareBuilder
                var validationFailures = new Blueprint.Validation.ValidationFailures();
                var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(addBackgroundTaskCommand);
                validationContext.MemberName = "Parameter";
                validationContext.DisplayName = "Parameter";

                // context.Descriptor.Properties[0] == AddBackgroundTaskCommand.Parameter
                foreach (var attribute in context.Descriptor.PropertyAttributes[0])
                {
                    if (attribute is System.ComponentModel.DataAnnotations.ValidationAttribute x)
                    {
                        var result =  x.GetValidationResult(addBackgroundTaskCommand.Parameter, validationContext);
                        if (result != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                        {
                            validationFailures.AddFailure(result);
                        }
                    }
                }
                if (validationFailures.Count > 0)
                {
                    _ValidationFailed(_logger, validationFailures.Count, null);
                    var validationFailedOperationResult = new Blueprint.Middleware.ValidationFailedOperationResult(validationFailures);
                    return validationFailedOperationResult;
                }

                // OperationExecutorMiddlewareBuilder
                using var activityOfAddBackgroundTaskCommand = Blueprint.Diagnostics.BlueprintActivitySource.ActivitySource.StartActivity("AddBackgroundTaskCommand", System.Diagnostics.ActivityKind.Internal);
                _OperationExecuting(_logger, "AddBackgroundTaskCommand", null);
                var backgroundTaskScheduler = context.ServiceProvider.GetRequiredService<Blueprint.Tasks.IBackgroundTaskScheduler>();
                var handlerResult = addBackgroundTaskCommand.Invoke(backgroundTaskScheduler);
                Blueprint.OperationResult operationResult = handlerResult;
                activityOfAddBackgroundTaskCommand?.Dispose();

                // ResourceEventHandlerMiddlewareBuilder
                var resourceEventRepository = context.ServiceProvider.GetRequiredService<Blueprint.Http.IResourceEventRepository>();
                var apiLinkGenerator = context.ServiceProvider.GetRequiredService<Blueprint.Http.IApiLinkGenerator>();
                await Blueprint.Http.Middleware.ResourceEventHandler.HandleAsync(resourceEventRepository, apiLinkGenerator, context, operationResult);

                // ReturnFrameMiddlewareBuilder
                return operationResult;
            }
            catch (Blueprint.Validation.ValidationException e)
            {
                context.Activity?.SetTag("otel.status_code", "OK");
                var validationFailedOperationResult = new Blueprint.Middleware.ValidationFailedOperationResult(e.ValidationResults);
                return validationFailedOperationResult;
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException e)
            {
                context.Activity?.SetTag("otel.status_code", "OK");
                var validationFailedOperationResult = Blueprint.Middleware.ValidationMiddlewareBuilder.ToValidationFailedOperationResult(e);
                return validationFailedOperationResult;
            }
            catch (System.Exception e)
            {
                if (e is Blueprint.ApiException apiException && apiException.HttpStatus < 500)
                {
                    _AnonxxApiExceptionhasoccurredwithmessageExceptionMessage(_logger, e.Message, e);
                    context.Activity?.SetTag("otel.status_code", "OK");
                }
                else
                {
                    Blueprint.Diagnostics.BlueprintActivitySource.RecordException(context.Activity, e, false);
                    _AnunhandledexceptionhasoccurredwithmessageExceptionMessage(_logger, e.Message, e);
                    context.Activity?.SetTag("otel.status_code", "ERROR");
                    context.Activity?.SetTag("otel.status_description", e.Message);
                }
                return new Blueprint.UnhandledExceptionOperationResult(e);
            }
        }
    }
}