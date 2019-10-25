using System;
using Blueprint.Core.Authorisation;
using Microsoft.AspNetCore.Http;

namespace Blueprint.Core.Errors
{
    /// <summary>
    /// Provides a means of recording exceptions that happen within an app that have not
    /// been caught and should not be ignored.
    /// </summary>
    /// <seealso cref="IExceptionFilter" />
    /// <seealso cref="IErrorLogger" />
    public interface IExceptionSink
    {
        void Record(Exception e, HttpContext httpContext, UserExceptionIdentifier identifier);
    }
}
