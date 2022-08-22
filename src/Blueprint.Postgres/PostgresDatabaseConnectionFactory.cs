using System.Data;
using Blueprint.Data;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Blueprint.Postgres;

/// <summary>
/// A <see cref="IDatabaseConnectionFactory" /> for Postgres.
/// </summary>
public class PostgresDatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresDatabaseConnectionFactory> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="PostgresDatabaseConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string to the Postgres database instance.</param>
    /// <param name="logger">A logger that will be used to log when a connection is opened.</param>
    public PostgresDatabaseConnectionFactory(string connectionString, ILogger<PostgresDatabaseConnectionFactory> logger)
    {
        Guard.NotNull(nameof(connectionString), connectionString);
        Guard.NotNull(nameof(logger), logger);

        this._connectionString = connectionString;
        this._logger = logger;
    }

    /// <inheritdoc/>
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