using Blueprint.Core;
using Blueprint.SqlServer;

// This is the recommendation from MS for extensions to IApplicationBuilder to aid discoverability
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
            string auditTableName)
        {
            Guard.NotNull(nameof(configurer), configurer);
            Guard.NotNullOrEmpty(nameof(connectionString), connectionString);
            Guard.NotNullOrEmpty(nameof(auditTableName), auditTableName);

            var databaseConnectionFactory = new SqlServerDatabaseConnectionFactory(connectionString);

            configurer.Use(new SqlServerAuditor(databaseConnectionFactory)
            {
                TableName = auditTableName
            });
        }
    }
}
