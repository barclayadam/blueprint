using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Utilities;

using NLog;

using Microsoft.AspNetCore.Http;

namespace Blueprint.Core.Errors
{
    public class ErrorLogger : IErrorLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly List<Exception> TestLoggedExceptions = new List<Exception>();
        private static bool isTestMode;

        private readonly IEnumerable<IErrorDataProvider> errorDataProviders;
        private readonly IEnumerable<IExceptionSink> exceptionSinks;
        private readonly IEnumerable<IExceptionFilter> exceptionFilters;

        public ErrorLogger(
            IEnumerable<IErrorDataProvider> errorDataProviders,
            IEnumerable<IExceptionSink> exceptionSinks,
            IEnumerable<IExceptionFilter> exceptionFilters)
        {
            this.errorDataProviders = errorDataProviders;
            this.exceptionSinks = exceptionSinks;
            this.exceptionFilters = exceptionFilters;
        }

        public static IEnumerable<Exception> LoggedExceptions => TestLoggedExceptions;

        /// <summary>
        /// Enters 'test mode' for the error logger, which will allow collecting of exceptions that
        /// have been logged to allow interrogating them at the end of a test. Note calling this
        /// method clears any previously logged exceptions.
        /// </summary>
        public static void EnterTestMode()
        {
            isTestMode = true;
            TestLoggedExceptions.Clear();
        }

        public bool ShouldIgnore(Exception exception)
        {
            var exceptionType = exception.GetType();

            foreach (var filter in exceptionFilters)
            {
                if (filter.ShouldIgnore(exceptionType, exception))
                {
                    return true;
                }
            }

            return false;
        }

        public ErrorLogStatus Log(string exceptionMessage, object errorData = default, HttpContext httpContext = default, UserExceptionIdentifier identifier = default)
        {
            return Log(new Exception(exceptionMessage), errorData, httpContext, identifier);
        }

        public ErrorLogStatus Log(Exception exception, object errorData = null, HttpContext httpContext = null, UserExceptionIdentifier userExceptionIdentifier = null)
        {
            if (ShouldIgnore(exception))
            {
                return ErrorLogStatus.Ignored;
            }

            exception = exception.Demystify();

            // This is not a known and well-handled exception
            Logger.Error(exception, "An unhandled exception has occurred");

            if (isTestMode)
            {
                TestLoggedExceptions.Add(exception);

                return ErrorLogStatus.Recorded;
            }

            var dataAsDictionary = errorData == null ? new Dictionary<string, string>() :
                errorData is Dictionary<string, string> data ? data : errorData.ToStringDictionary();

            try
            {
                dataAsDictionary = Populate(dataAsDictionary);

                foreach (var sink in exceptionSinks)
                {
                    sink.Record(exception, dataAsDictionary, httpContext, userExceptionIdentifier);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error logging error");
                Logger.Error(exception, "Original error");
            }

            return ErrorLogStatus.Recorded;
        }

        /// <summary>
        /// Given the error data passed by the caller of the error logging methods will give data providers
        /// the chance to populate extra information from the environment.
        /// </summary>
        /// <param name="errorData">The original error data given to the logging methods.</param>
        /// <returns>A non-null dictionary of error data, including that from registered providers.</returns>
        private Dictionary<string, string> Populate(Dictionary<string, string> errorData)
        {
            foreach(var provider in errorDataProviders)
            {
                provider.Populate(errorData);
            }

            return errorData;
        }
    }
}
