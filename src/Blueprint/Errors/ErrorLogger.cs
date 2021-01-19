using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Authorisation;
using Blueprint.Utilities;
using Microsoft.Extensions.Logging;

namespace Blueprint.Errors
{
    /// <summary>
    /// The default implementation of <see cref="IErrorLogger" /> that pushes exceptions to a number of
    /// <see cref="IExceptionSink" />s.
    /// </summary>
    public class ErrorLogger : IErrorLogger
    {
        private static readonly List<Exception> _testLoggedExceptions = new List<Exception>();
        private static bool _isTestMode;

        private readonly IEnumerable<IErrorDataProvider> _errorDataProviders;
        private readonly IEnumerable<IExceptionSink> _exceptionSinks;
        private readonly IEnumerable<IExceptionFilter> _exceptionFilters;
        private readonly ILogger<ErrorLogger> _logger;

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
            this._errorDataProviders = errorDataProviders;
            this._exceptionSinks = exceptionSinks;
            this._exceptionFilters = exceptionFilters;
            this._logger = logger;
        }

        public static IEnumerable<Exception> LoggedExceptions => _testLoggedExceptions;

        /// <summary>
        /// Enters 'test mode' for the error logger, which will allow collecting of exceptions that
        /// have been logged to allow interrogating them at the end of a test. Note calling this
        /// method clears any previously logged exceptions.
        /// </summary>
        public static void EnterTestMode()
        {
            _isTestMode = true;
            _testLoggedExceptions.Clear();
        }

        /// <inheritdoc />
        public bool ShouldIgnore(Exception exception)
        {
            var exceptionType = exception.GetType();

            foreach (var filter in this._exceptionFilters)
            {
                if (filter.ShouldIgnore(exceptionType, exception))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public ValueTask<ErrorLogStatus> LogAsync(string exceptionMessage, object errorData = default, UserExceptionIdentifier identifier = default)
        {
            return this.LogAsync(new Exception(exceptionMessage), errorData, identifier);
        }

        /// <inheritdoc />
        public async ValueTask<ErrorLogStatus> LogAsync(Exception exception, object errorData = default, UserExceptionIdentifier userExceptionIdentifier = null)
        {
            if (this.ShouldIgnore(exception))
            {
                return ErrorLogStatus.Ignored;
            }

            // Attach extra data to the Exception object via. it's Data property, used in exception sinks to
            // add metadata useful for diagnostics purposes.
            if (errorData != null)
            {
                var dataAsDictionary = errorData is Dictionary<string, string> data ? data : errorData.ToStringDictionary();

                foreach (var kvp in dataAsDictionary)
                {
                    exception.Data[kvp.Key] = kvp.Value;
                }
            }

            // This is not a known and well-handled exception
            this._logger.LogError(exception, "An unhandled exception has occurred: {Message}", exception.Message);

            if (_isTestMode)
            {
                _testLoggedExceptions.Add(exception);

                return ErrorLogStatus.Recorded;
            }

            try
            {
                this.Populate(exception);

                foreach (var sink in this._exceptionSinks)
                {
                    await sink.RecordAsync(exception, userExceptionIdentifier);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error logging error");
                this._logger.LogError(exception, "Original error");
            }

            return ErrorLogStatus.Recorded;
        }

        private void Populate(Exception exception)
        {
            foreach (var provider in this._errorDataProviders)
            {
                provider.Populate(exception.Data);
            }
        }
    }
}
