using Blueprint;
using Blueprint.Configuration;
using Blueprint.Data;
using Blueprint.SqlServer;
using Microsoft.Extensions.Logging;

// Match the DI container namespace so that Blueprint is immediately discoverable
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class BlueprintAuditBuilderExtensions
    {
        /// <summary>
        /// Configures Blueprint to use SQL Server to store audit information, using the tables specified to store data.
        /// </summary>
        public static void StoreInSqlServer<THost>(
            this BlueprintAuditBuilder<THost> builder,
            string connectionString,
            string tableName)
        {
            Guard.NotNullOrEmpty(nameof(connectionString), connectionString);
            Guard.NotNullOrEmpty(nameof(tableName), tableName);

            builder.Services.Configure<SqlServerAuditorConfiguration>(c =>
            {
                c.QualifiedTableName = TableName.Parse(tableName).QualifiedTableName;
            });

            builder.Services.AddScoped<IDatabaseConnectionFactory>(
                s => new SqlServerDatabaseConnectionFactory(connectionString, s.GetRequiredService<ILogger<SqlServerDatabaseConnectionFactory>>()));

            builder.UseAuditor<SqlServerAuditor>();
        }

        /// <summary>
        /// Configures Blueprint to use SQL Server to store audit information, using the table specified to store data. This will configure the auditor
        /// and <see cref="SqlServerAuditorConfiguration" />, but relies on a <see cref="IDatabaseConnectionFactory" /> registration being made
        /// elsewhere.
        /// </summary>
        /// <typeparam name="THost">The type of host.</typeparam>
        public static void StoreInSqlServer<THost>(
            this BlueprintAuditBuilder<THost> builder,
            string tableName)
        {
            Guard.NotNullOrEmpty(nameof(tableName), tableName);

            builder.Services.Configure<SqlServerAuditorConfiguration>(c =>
            {
                c.QualifiedTableName = TableName.Parse(tableName).QualifiedTableName;
            });

            builder.UseAuditor<SqlServerAuditor>();
        }
    }
}
