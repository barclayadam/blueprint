using System;
using System.Security;
using System.Web;
using Blueprint.Core.Validation;
using NLog;

namespace Blueprint.Core.Errors
{
    public class BasicExceptionFilter : IExceptionFilter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Type TypeValidationError = typeof(ValidationException);
        private static readonly Type TypeNotFoundException = typeof(NotFoundException);
        private static readonly Type TypeSecurityException = typeof(SecurityException);
        private static readonly Type TypeForbiddenException = typeof(ForbiddenException);
        private static readonly Type TypeHttpException = typeof(HttpException);

        public bool ShouldIgnore(Type exceptionType, Exception exception)
        {
            if (exceptionType == TypeValidationError)
            {
                Logger.Info("Request has failed validation");

                return true;
            }

            if (exceptionType == TypeNotFoundException)
            {
                Logger.Info("404. Could not find entity");

                return true;
            }

            if (exceptionType == TypeSecurityException)
            {
                Logger.Info("Request has failed authorisation. message='{0}'", exception.Message);

                return true;
            }

            if (exceptionType == TypeForbiddenException)
            {
                Logger.Info("Request is forbidden. message='{0}'", exception.Message);

                return true;
            }

            if (exceptionType == TypeHttpException)
            {
                Logger.Info("HTTP error response. status_code={0} message='{1}'",
                            ((HttpException)exception).GetHttpCode(),
                            exception.Message);

                return true;
            }

            return false;
        }
    }
}