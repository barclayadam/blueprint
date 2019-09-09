using System.Collections.Generic;
using System.Transactions;
using Blueprint.Core.Data;
using Blueprint.Core.Utilities;
using Blueprint.SqlServer;
using Blueprint.Testing;
using Dapper;
using NUnit.Framework;

namespace Blueprint.Tests
{
    [Category("Integration")]
    public class With_Local_Database
    {
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=blueprint;Integrated Security=True";

        protected IDatabaseConnectionFactory CreateFactory() =>
            new SqlServerDatabaseConnectionFactory(ConnectionString);

        [SetUp]
        public void SetUpDatabase()
        {
            SqlLocalDbDatabaseCreator.Recreate(ConnectionString);

            using (var cn = CreateFactory().Open())
            using (var transaction = new TransactionScope())
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
    }
}
