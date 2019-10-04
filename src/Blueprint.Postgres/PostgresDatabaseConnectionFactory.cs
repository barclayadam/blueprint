using System.Data;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Blueprint.Core.Data
{
    public class PostgresDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly string connectionString;
        private readonly ILogger<PostgresDatabaseConnectionFactory> logger;

        public PostgresDatabaseConnectionFactory(string connectionString, ILogger<PostgresDatabaseConnectionFactory> logger)
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

            var connection = new NpgsqlConnection(connectionString);

            connection.Open();

            return connection;
        }
    }
}
