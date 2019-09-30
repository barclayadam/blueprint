using System.Data;
using System.Data.SqlClient;
using Blueprint.Core;
using Blueprint.Core.Data;
using NLog;

namespace Blueprint.SqlServer
{
    public class SqlServerDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly string connectionString;

        public SqlServerDatabaseConnectionFactory(string connectionString)
        {
            Guard.NotNull(nameof(connectionString), connectionString);

            this.connectionString = connectionString;
        }

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
