using System;
using NHibernate;
using NLog;

namespace Blueprint.NHibernate.Data.NHibernate
{
    /// <summary>
    /// Provides an <see cref="ILoggerFactory" /> that bridges NHibernate logging to
    /// NLog, using a single logger name of 'nhibernate' that can be targeted for
    /// configuration.
    /// </summary>
    public class NLogFactory : ILoggerFactory
    {
        private const string NHibernateLoggerName = "NHibernate";

        /// <summary>
        /// Gets a <see cref="NLogLogger"/> for the specified type, which will be
        /// the single logger with the name of <c>NHibernate</c>.
        /// </summary>
        /// <param name="type">The type for the logger, currently ignored.</param>
        /// <returns>The single NLog logger implementation of <see cref="IInternalLogger"/>.</returns>
        public IInternalLogger LoggerFor(Type type)
        {
            return new NLogLogger(LogManager.GetLogger(NHibernateLoggerName + "." + type.Name));
        }

        /// <summary>
        /// Gets a <see cref="NLogLogger"/> for the specified type, which will be
        /// the single logger with the name of <c>NHibernate</c>.
        /// </summary>
        /// <param name="keyName">The key for the logger, currently ignored.</param>
        /// <returns>The single NLog logger implementation of <see cref="IInternalLogger"/>.</returns>
        public IInternalLogger LoggerFor(string keyName)
        {
            return new NLogLogger(LogManager.GetLogger(keyName));
        }
    }
}
