using System.Data;
using System.Data.SqlClient;
using Blueprint;
using Blueprint.Data;
using Microsoft.Extensions.Logging;

namespace Blueprint.SqlServer
{
    public class SqlServerDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string connectionString;
        private readonly ILogger<SqlServerDatabaseConnectionFactory> logger;

        public SqlServerDatabaseConnectionFactory(string connectionString, ILogger<SqlServerDatabaseConnectionFactory> logger)
        {
            Guard.NotNull(nameof(connectionString), connectionString);
            Guard.NotNull(nameof(logger), logger);

            this.connectionString = connectionString;
            this.logger = logger;
        }

        public IDbConnection Open()
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Opening new SQL DB connection");
            }

            var connection = new SqlConnection(connectionString);

            connection.Open();

            return connection;
        }
    }
}
