using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Transactions;
using Blueprint.Data;
using Blueprint.Utilities;
using Blueprint.SqlServer;
using Dapper;
using NUnit.Framework;
using IsolationLevel = System.Data.IsolationLevel;

namespace Blueprint.Tests;

[Category("Integration")]
public class With_Local_Database
{
    private PerTestSqlLiteDatabaseConnectionFactory factory;

    protected IDatabaseConnectionFactory CreateFactory() => factory;

    [SetUp]
    public void SetUpDatabase()
    {
        factory?.Dispose();
        factory = new PerTestSqlLiteDatabaseConnectionFactory("Data Source=:memory:;Version=3");

        using (var transaction = new TransactionScope())
        using (var cn = CreateFactory().Open())
        {
            cn.SplitAndExecuteSql(typeof(With_Local_Database).GetRelativeEmbeddedResourceAsString("BlueprintSqlSchema.sql"));

            transaction.Complete();
        }
    }

    protected IEnumerable<T> Query<T>(string sql, object param = null)
    {
        using (var cn = CreateFactory().Open())
        {
            return cn.Query<T>(sql, param);
        }
    }

    /// <summary>
    /// Because SQLite destroys in-memory databases when a connection is closed we use a non-disposing
    /// wrapping connection over a single opened SQLiteConnection that is created once per test (in this
    /// factory), and then disposed once the next test that requires it.
    /// </summary>
    /// <remarks>
    /// This is required because normally users of the factory expect to be given a connection that they
    /// use for a short period before disposing, as is correct in most other cases of SQL connections (i.e. returning
    /// them to the pool).
    ///
    /// If we were to allow the connection to be disposed it could only be used once before the data and schema etc.
    /// already setup were destroyed.
    /// </remarks>
    private class PerTestSqlLiteDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly IDbConnection openConnection;
        private readonly IDbConnection wrappedOpenConnection;

        public PerTestSqlLiteDatabaseConnectionFactory(string connectionString)
        {
            openConnection = new SQLiteConnection(connectionString).OpenAndReturn();
            wrappedOpenConnection = new NonDisposingIDbConnection(openConnection);
        }

        public IDbConnection Open()
        {
            return wrappedOpenConnection;
        }

        public void Dispose()
        {
            openConnection.Dispose();
        }
    }

    private class NonDisposingIDbConnection : IDbConnection
    {
        private readonly IDbConnection dbConnectionImplementation;

        public NonDisposingIDbConnection(IDbConnection dbConnectionImplementation)
        {
            this.dbConnectionImplementation = dbConnectionImplementation;
        }

        public void Dispose()
        {
            // Do not do anything here
        }

        public IDbTransaction BeginTransaction()
        {
            return dbConnectionImplementation.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return dbConnectionImplementation.BeginTransaction(il);
        }

        public void Close()
        {
            dbConnectionImplementation.Close();
        }

        public void ChangeDatabase(string databaseName)
        {
            dbConnectionImplementation.ChangeDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            return dbConnectionImplementation.CreateCommand();
        }

        public void Open()
        {
            dbConnectionImplementation.Open();
        }

        public string ConnectionString
        {
            get => dbConnectionImplementation.ConnectionString;
            set => dbConnectionImplementation.ConnectionString = value;
        }

        public int ConnectionTimeout => dbConnectionImplementation.ConnectionTimeout;

        public string Database => dbConnectionImplementation.Database;

        public ConnectionState State => dbConnectionImplementation.State;
    }
}