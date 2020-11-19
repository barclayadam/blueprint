using System;
using System.Security;
using Blueprint.Validation;
using Microsoft.Extensions.Logging;

namespace Blueprint.Errors
{
    public class BasicExceptionFilter : IExceptionFilter
    {
        private static readonly Type _typeValidationError = typeof(ValidationException);
        private static readonly Type _typeNotFoundException = typeof(NotFoundException);
        private static readonly Type _typeSecurityException = typeof(SecurityException);
        private static readonly Type _typeForbiddenException = typeof(ForbiddenException);

        private readonly ILogger<BasicExceptionFilter> _logger;

        public BasicExceptionFilter(ILogger<BasicExceptionFilter> logger)
        {
            this._logger = logger;
        }

        public bool ShouldIgnore(Type exceptionType, Exception exception)
        {
            if (exceptionType == _typeValidationError)
            {
                this._logger.LogInformation("Request has failed validation");

                return true;
            }

            if (exceptionType == _typeNotFoundException)
            {
                this._logger.LogInformation("404. Could not find entity");

                return true;
            }

            if (exceptionType == _typeSecurityException)
            {
                this._logger.LogInformation("Request has failed authorisation. message='{0}'", exception.Message);

                return true;
            }

            if (exceptionType == _typeForbiddenException)
            {
                this._logger.LogInformation("Request is forbidden. message='{0}'", exception.Message);

                return true;
            }

            return false;
        }
    }
}
