using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Blueprint.Core.Data;
using Dapper;
using NLog;

namespace Blueprint.Testing
{
    /// <summary>
    /// Provides the ability to create Postgres databases to be used within automated tests.
    /// </summary>
    public static class PostgresDbDatabaseCreator
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly Regex DatabaseNameRegex =
            new Regex("database=(?<databaseName>.*?)(;|$)", RegexOptions.Compiled);

        /// <summary>
        /// Creates a new LocalDb database, not dropping an existing database if it exists.
        /// </summary>
        public static string CreateIfRequired(string connectionStringSettings)
        {
            CreateDatabaseIfRequired(connectionStringSettings);

            return connectionStringSettings;
        }

        /// <summary>
        /// Creates a new Postgres database, dropping any existing database with the same name.
        /// </summary>
        public static string Recreate(string connectionStringSettings)
        {
            DropDatabaseIfItExists(connectionStringSettings);
            CreateDatabaseIfRequired(connectionStringSettings);

            return connectionStringSettings;
        }

        /// <summary>
        /// Creates a new Postgres database, dropping any existing database with the same name.
        /// </summary>
        public static bool RecreateIfSchemaDifferent(string connectionString, string schemaKey)
        {
            var schemaHash = HashStringMD5(schemaKey);
            var databaseName = DatabaseNameRegex.Match(connectionString).Groups["databaseName"].Value;
            var shouldCreate = false;

            using (var connection = OpenRootConnection(connectionString))
            {
                var result = connection.Query($@"select pg_database.oid, pg_shdescription.description from pg_database
                    left join pg_shdescription on objoid = pg_database.oid
                    where datname = '{databaseName}'").FirstOrDefault();

                if (result == null)
                {
                    Log.Info($"Database {databaseName} does not exist. Creating");

                    // Database does not exist at all
                    shouldCreate = true;
                }
                else if (schemaHash != result.description)
                {
                    Log.Info($"Database {databaseName} has different schema. existing_hash={result.description} new_hash={schemaHash}. Recreating");

                    // Database does exist, but it's out of date
                    shouldCreate = true;

                    connection.Execute($"select pid, pg_terminate_backend(pid) FROM pg_stat_activity where datname = '{databaseName}' AND pid <> pg_backend_pid();");
                    connection.Execute($"drop database \"{databaseName}\";");
                }
            }

            using (var connection = OpenRootConnection(connectionString))
            {
                if (shouldCreate)
                {
                    connection.Execute($"CREATE DATABASE \"{databaseName}\"");
                    connection.Execute($"COMMENT ON DATABASE \"{databaseName}\" IS '{schemaHash}';");

                    // Test databases do not need the same guarantees as production, so improve performance by
                    // allowing async commits to disk. A crash means potentially lost transactions, but tests wipe out
                    // on each run anyway
                    connection.Execute($"ALTER DATABASE \"{databaseName}\" SET synchronous_commit TO off");
                }
                else
                {
                    Log.Info($"Database {databaseName} schema is up to date. Not recreating");
                }
            }

            return shouldCreate;
        }

        private static void DropDatabaseIfItExists(string connectionString)
        {
            var databaseName = DatabaseNameRegex.Match(connectionString).Groups["databaseName"].Value;

            using (var connection = OpenRootConnection(connectionString))
            {
                var result = connection.Query<short?>($"SELECT 1 FROM pg_database WHERE datname='{databaseName}'").FirstOrDefault();

                if (result != null)
                {
                    connection.Execute(
                        $"select pid, pg_terminate_backend(pid) FROM pg_stat_activity where datname = '{databaseName}' AND pid <> pg_backend_pid();");
                    connection.Execute($"drop database \"{databaseName}\";");
                }
            }
        }

        private static void CreateDatabaseIfRequired(string connectionString)
        {
            var databaseName = DatabaseNameRegex.Match(connectionString).Groups["databaseName"].Value;

            using (var connection = OpenRootConnection(connectionString))
            {
                var result = connection.Query<short?>($"SELECT 1 FROM pg_database WHERE datname='{databaseName}'").FirstOrDefault();

                if (result == null)
                {
                    connection.Execute($"CREATE DATABASE \"{databaseName}\"");

                    // Test databases do not need the same guarantees as production, so improve performance by
                    // allowing async commits to disk. A crash means potentially lost transactions, but tests wipe out
                    // on each run anyway
                    connection.Execute($"ALTER DATABASE \"{databaseName}\" SET synchronous_commit TO off");
                }
            }
        }

        private static IDbConnection OpenRootConnection(string connectionString)
        {
            var withoutDatabaseName = DatabaseNameRegex.Replace(connectionString, "");

            return new PostgresDatabaseConnectionFactory(withoutDatabaseName).Open();
        }

        private static string HashStringMD5(string value)
        {
            var data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(value));

            return data.Aggregate(string.Empty, (current, t) => current + t.ToString("x2").ToLowerInvariant());
        }
    }
}
