using System;
using System.Threading.Tasks;
using Blueprint.Core.Authorisation;

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
        Task RecordAsync(Exception e, UserExceptionIdentifier identifier);
    }
}
