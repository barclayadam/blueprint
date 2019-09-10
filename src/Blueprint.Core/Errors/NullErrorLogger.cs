using System;
using Blueprint.Core.Authorisation;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Core.Errors
{
    public class NullErrorLogger : IErrorLogger
    {
        public ErrorLogStatus Log(Exception exception, object errorData = default, HttpContext httpContext = default, UserExceptionIdentifier identifier = default)
        {
            return ErrorLogStatus.Recorded;
        }

        public ErrorLogStatus Log(string exceptionMessage, object errorData = default, HttpContext httpContext = default, UserExceptionIdentifier identifier = default)
        {
            return ErrorLogStatus.Recorded;
        }

        public bool ShouldIgnore(Exception exception)
        {
            return false;
        }
    }
}
