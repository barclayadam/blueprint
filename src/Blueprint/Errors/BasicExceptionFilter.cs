using System;
using System.Security;
using Blueprint.Validation;
using Microsoft.Extensions.Logging;

namespace Blueprint.Errors
{
    public class BasicExceptionFilter : IExceptionFilter
    {
        private static readonly Type TypeValidationError = typeof(ValidationException);
        private static readonly Type TypeNotFoundException = typeof(NotFoundException);
        private static readonly Type TypeSecurityException = typeof(SecurityException);
        private static readonly Type TypeForbiddenException = typeof(ForbiddenException);

        private readonly ILogger<BasicExceptionFilter> logger;

        public BasicExceptionFilter(ILogger<BasicExceptionFilter> logger)
        {
            this.logger = logger;
        }

        public bool ShouldIgnore(Type exceptionType, Exception exception)
        {
            if (exceptionType == TypeValidationError)
            {
                logger.LogInformation("Request has failed validation");

                return true;
            }

            if (exceptionType == TypeNotFoundException)
            {
                logger.LogInformation("404. Could not find entity");

                return true;
            }

            if (exceptionType == TypeSecurityException)
            {
                logger.LogInformation("Request has failed authorisation. message='{0}'", exception.Message);

                return true;
            }

            if (exceptionType == TypeForbiddenException)
            {
                logger.LogInformation("Request is forbidden. message='{0}'", exception.Message);

                return true;
            }

            return false;
        }
    }
}
