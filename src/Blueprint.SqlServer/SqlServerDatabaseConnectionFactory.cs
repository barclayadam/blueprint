using System.Data;
using System.Data.SqlClient;
using Blueprint.Data;
using Microsoft.Extensions.Logging;

namespace Blueprint.SqlServer
{
    public class SqlServerDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlServerDatabaseConnectionFactory> _logger;

        public SqlServerDatabaseConnectionFactory(string connectionString, ILogger<SqlServerDatabaseConnectionFactory> logger)
        {
            Guard.NotNull(nameof(connectionString), connectionString);
            Guard.NotNull(nameof(logger), logger);

            this._connectionString = connectionString;
            this._logger = logger;
        }

        public IDbConnection Open()
        {
            if (this._logger.IsEnabled(LogLevel.Trace))
            {
                this._logger.LogTrace("Opening new SQL DB connection");
            }

            var connection = new SqlConnection(this._connectionString);

            connection.Open();

            return connection;
        }
    }
}
