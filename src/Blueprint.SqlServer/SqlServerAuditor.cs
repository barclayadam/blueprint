using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueprint.Auditing;
using Blueprint.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Blueprint.SqlServer
{
    /// <summary>
    /// Provides the ability to persist <see cref="AuditItem"/>s to a database.
    /// </summary>
    public class SqlServerAuditor : IAuditor
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            ContractResolver = new AuditDetailsResolver(),
        };

        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
        private readonly IOptions<SqlServerAuditorConfiguration> _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerAuditor"/> class.
        /// </summary>
        public SqlServerAuditor(
            IDatabaseConnectionFactory databaseConnectionFactory,
            IOptions<SqlServerAuditorConfiguration> configuration)
        {
            Guard.NotNull(nameof(databaseConnectionFactory), databaseConnectionFactory);

            this._databaseConnectionFactory = databaseConnectionFactory;
            this._configuration = configuration;
        }

        /// <summary>
        /// Insert the audit item into the database.
        /// </summary>
        /// <param name="auditItem">The audit item to insert.</param>
        public void Write(AuditItem auditItem)
        {
            var valueToSerialize = auditItem.Details;
            var type = auditItem.Details.GetType().Name;

            var serializedMessage = JsonConvert.SerializeObject(
                                                                valueToSerialize,
                                                                Formatting.None,
                                                                _jsonSerializerSettings);
            using (var cn = this._databaseConnectionFactory.Open())
            using (var transaction = cn.BeginTransaction())
            {
                cn.Execute(
                    $@"INSERT INTO {this._configuration.Value.QualifiedTableName} (CorrelationId,  WasSuccessful,  ResultMessage,  Username,  Timestamp,  MessageType,  MessageData)
                           VALUES (@CorrelationId, @WasSuccessful, @ResultMessage, @Username, @Timestamp, @MessageType, @MessageData)",
                    new
                    {
                        auditItem.CorrelationId,
                        auditItem.ResultMessage,
                        auditItem.Username,
                        auditItem.WasSuccessful,
                        Timestamp = SystemTime.UtcNow,
                        MessageType = type,
                        MessageData = serializedMessage,
                    },
                    transaction);

                transaction.Commit();
            }
        }

        /// <summary>
        /// A contract resolver that will filter out properties that have <see cref="DoNotAuditAttribute"/>
        /// or <see cref="SensitiveAttribute"/> applied.
        /// </summary>
        private class AuditDetailsResolver : DefaultContractResolver
        {
            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                return base.GetSerializableMembers(objectType)
                           .Where(p => !SensitiveProperties.IsSensitive(p))
                           .ToList();
            }
        }
    }
}
