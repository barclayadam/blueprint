using System.Data;
using Blueprint.Data;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Blueprint.Postgres
{
    public class PostgresDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string _connectionString;
        private readonly ILogger<PostgresDatabaseConnectionFactory> _logger;

        public PostgresDatabaseConnectionFactory(string connectionString, ILogger<PostgresDatabaseConnectionFactory> logger)
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

            var connection = new NpgsqlConnection(this._connectionString);

            connection.Open();

            return connection;
        }
    }
}
