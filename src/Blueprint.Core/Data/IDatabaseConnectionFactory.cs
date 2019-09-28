using System;
using System.Data;

namespace Blueprint.Core.Data
{
    /// <summary>
    /// Represents a small facade over the creation &amp; opening of a database
    /// connection to avoid relying directly on <see cref="IDbConnection" /> or
    /// having to take a <see cref="Func{TResult} "/> to create the connection.
    /// </summary>
    public interface IDatabaseConnectionFactory
    {
        /// <summary>
        /// Opens a new connection to the database, which should be done for small
        /// operations in a `using` clause.
        /// </summary>
        /// <remarks>
        /// It is the complete responsibility of clients of this method to manage the connection once it has
        /// been opened, to dispose and close the connection when done with.
        /// </remarks>
        /// <returns>An opened database connection.</returns>
        IDbConnection Open();
    }
}
