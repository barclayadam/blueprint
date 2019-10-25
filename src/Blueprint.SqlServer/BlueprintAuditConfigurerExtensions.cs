using Blueprint.Core;
using Blueprint.Core.Data;
using Blueprint.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// This should be discoverable when configuring without extra namespace imports
// ReSharper disable once CheckNamespace
namespace Blueprint.Api.Configuration
{
    public static class BlueprintAuditConfigurerExtensions
    {
        /// <summary>
        /// Configures Blueprint to use SQL Server to store audit information, using the tables specified to store data.
        /// </summary>
        public static void StoreInSqlServer(
            this BlueprintAuditConfigurer configurer,
            string connectionString,
            string tableName)
        {
            Guard.NotNullOrEmpty(nameof(connectionString), connectionString);
            Guard.NotNullOrEmpty(nameof(tableName), tableName);

            configurer.Services.Configure<SqlServerAuditorConfiguration>(c =>
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    return;
                }

                c.QualifiedTableName = TableName.Parse(tableName).QualifiedTableName;
            });

            configurer.Services.AddScoped<IDatabaseConnectionFactory>(
                s => new SqlServerDatabaseConnectionFactory(connectionString, s.GetRequiredService<ILogger<SqlServerDatabaseConnectionFactory>>()));

            configurer.UseAuditor<SqlServerAuditor>();
        }
    }
}
