using System.Data;
using System.Data.SqlClient;
using Blueprint.Data;
using Microsoft.Extensions.Logging;

namespace Blueprint.SqlServer;

/// <summary>
/// An <see cref="IDatabaseConnectionFactory" /> for SQL Server.
/// </summary>
public class SqlServerDatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerDatabaseConnectionFactory> _logger;

    /// <summary>
    /// Initialises a new instance of the <see cref="SqlServerDatabaseConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string of the database.</param>
    /// <param name="logger">A logger.</param>
    public SqlServerDatabaseConnectionFactory(string connectionString, ILogger<SqlServerDatabaseConnectionFactory> logger)
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

        var connection = new SqlConnection(this._connectionString);

        connection.Open();

        return connection;
    }
}