using Blueprint.Core;
using Blueprint.Core.Data;
using Blueprint.SqlServer;
using Microsoft.Extensions.DependencyInjection;

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
            string tableName,
            bool automaticallyCreateTables = true)
        {
            Guard.NotNull(nameof(configurer), configurer);
            Guard.NotNullOrEmpty(nameof(connectionString), connectionString);

            configurer.Services.AddOptions();
            configurer.Services.Configure<SqlServerAuditorConfiguration>(c =>
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    return;
                }

                c.QualifiedTableName = TableName.Parse(tableName).QualifiedTableName;
            });

            configurer.Services.AddScoped<IDatabaseConnectionFactory>(s => new SqlServerDatabaseConnectionFactory(connectionString));

            configurer.UseAuditor<SqlServerAuditor>();
        }
    }
}
