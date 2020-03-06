using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace Blueprint.Core.Errors
{
    /// <summary>
    /// The default implementation of <see cref="IErrorLogger" /> that pushes exceptions to a number of
    /// <see cref="IExceptionSink" />s.
    /// </summary>
    public class ErrorLogger : IErrorLogger
    {
        private static readonly List<Exception> TestLoggedExceptions = new List<Exception>();
        private static bool isTestMode;

        private readonly IEnumerable<IErrorDataProvider> errorDataProviders;
        private readonly IEnumerable<IExceptionSink> exceptionSinks;
        private readonly IEnumerable<IExceptionFilter> exceptionFilters;
        private readonly ILogger<ErrorLogger> logger;

        /// <summary>
        /// Initialises a new instance of the <see cref="ErrorLogger" /> class.
        /// </summary>
        /// <param name="errorDataProviders">The data providers that can add ambient data to logged exceptions.</param>
        /// <param name="exceptionSinks">The sinks that exceptions should be pushed to.</param>
        /// <param name="exceptionFilters">Filters that are used to exclude exceptions from logging.</param>
        /// <param name="logger">A logger that will, as a last resort, be sent exceptions if it is not possible to log using configured sinks.</param>
        public ErrorLogger(
            IEnumerable<IErrorDataProvider> errorDataProviders,
            IEnumerable<IExceptionSink> exceptionSinks,
            IEnumerable<IExceptionFilter> exceptionFilters,
            ILogger<ErrorLogger> logger)
        {
            this.errorDataProviders = errorDataProviders;
            this.exceptionSinks = exceptionSinks;
            this.exceptionFilters = exceptionFilters;
            this.logger = logger;
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public ErrorLogStatus Log(string exceptionMessage, object errorData = default, UserExceptionIdentifier identifier = default)
        {
            var exception = new Exception(exceptionMessage);

            if (errorData != null)
            {
                var dataAsDictionary = errorData is Dictionary<string, string> data ? data : errorData.ToStringDictionary();

                foreach (var kvp in dataAsDictionary)
                {
                    exception.Data[kvp.Key] = kvp.Value;
                }
            }

            return Log(exception, identifier);
        }

        /// <inheritdoc />
        public ErrorLogStatus Log(Exception exception, UserExceptionIdentifier userExceptionIdentifier = null)
        {
            if (ShouldIgnore(exception))
            {
                return ErrorLogStatus.Ignored;
            }

            exception = exception.Demystify();

            // This is not a known and well-handled exception
            logger.LogError(exception, "An unhandled exception has occurred");

            if (isTestMode)
            {
                TestLoggedExceptions.Add(exception);

                return ErrorLogStatus.Recorded;
            }

            try
            {
                Populate(exception);

                foreach (var sink in exceptionSinks)
                {
                    sink.Record(exception, userExceptionIdentifier);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error logging error");
                logger.LogError(exception, "Original error");
            }

            return ErrorLogStatus.Recorded;
        }

        private void Populate(Exception exception)
        {
            foreach (var provider in errorDataProviders)
            {
                provider.Populate(exception.Data);
            }
        }
    }
}
