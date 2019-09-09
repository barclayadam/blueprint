﻿using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Blueprint.Core.Dapper;
using Blueprint.Core.Data;
using Blueprint.Core.Utilities;

namespace Blueprint.Testing
{
    /// <summary>
    /// Provides the ability to create LocalDb databases to be used within automated tests.
    /// </summary>
    public static class SqlLocalDbDatabaseCreator
    {
        private static readonly Regex InitialCatalogRegex =
            new Regex("Initial Catalog=(?<databaseName>.*?)(;|$)", RegexOptions.Compiled);

        /// <summary>
        /// Creates a new LocalDb database, not dropping an existing database if it exists.
        /// </summary>
        public static string Create(string connectionStringSettings)
        {
            CreateDatabaseIfRequired(connectionStringSettings);

            return connectionStringSettings;
        }

        /// <summary>
        /// Creates a new LocalDb database, dropping any existing database with the same name.
        /// </summary>
        public static string Recreate(string connectionStringSettings)
        {
            DropDatabaseIfItExists(connectionStringSettings);
            CreateDatabaseIfRequired(connectionStringSettings);

            return connectionStringSettings;
        }

        private static void DropDatabaseIfItExists(string connectionString)
        {
            var databaseName = InitialCatalogRegex.Match(connectionString).Groups["databaseName"].Value;

            using (var connection = OpenRootConnection(connectionString))
            {
                var result = connection.Query<short?>("SELECT DB_ID(N'{0}')".Fmt(databaseName)).FirstOrDefault();

                if (result != null)
                {
                    connection.Execute(@"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;".Fmt(databaseName));
                    connection.Execute(@"DROP DATABASE [{0}];".Fmt(databaseName));
                }
            }
        }

        private static void CreateDatabaseIfRequired(string connectionString)
        {
            var databaseName = InitialCatalogRegex.Match(connectionString).Groups["databaseName"].Value;

            using (var connection = OpenRootConnection(connectionString))
            {
                var result = connection.Query<short?>("SELECT DB_ID(N'{0}')".Fmt(databaseName)).FirstOrDefault();

                if (result == null)
                {
                    connection.Execute(@"CREATE DATABASE [{0}]".Fmt(databaseName));
                    connection.Execute(@"ALTER DATABASE [{0}] SET TRUSTWORTHY ON;".Fmt(databaseName));
                }
            }
        }

        private static IDbConnection OpenRootConnection(string connectionString)
        {
            var withoutInitialCatalog = InitialCatalogRegex.Replace(connectionString, "");

            return new MssqlDatabaseConnectionFactory(withoutInitialCatalog).Open();
        }
    }
}