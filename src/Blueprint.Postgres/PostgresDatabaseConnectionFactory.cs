using System.Data;

using NLog;

using Npgsql;

namespace Blueprint.Core.Data
{
    public class PostgresDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly string connectionString;

        public PostgresDatabaseConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbConnection Open()
        {
            if (Log.IsTraceEnabled)
            {
                Log.Trace("Opening new Postgres SQL DB connection");
            }

            var connection = new NpgsqlConnection(connectionString);

            connection.Open();

            return connection;
        }
    }
}
