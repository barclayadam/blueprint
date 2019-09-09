using System.Data;
using System.Data.SqlClient;

using NLog;

namespace Blueprint.Core.Data
{
    public class MssqlDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly string connectionString;

        public MssqlDatabaseConnectionFactory(string connectionString)
        {
            Guard.NotNull(nameof(connectionString), connectionString);

            this.connectionString = connectionString;
        }

        /// <inherit-doc />
        public IDbConnection Open()
        {
            if (Log.IsTraceEnabled)
            {
                Log.Trace("Opening new SQL DB connection");
            }

            var connection = new SqlConnection(connectionString);

            connection.Open();

            return connection;
        }
    }
}