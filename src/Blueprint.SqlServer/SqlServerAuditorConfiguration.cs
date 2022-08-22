namespace Blueprint.SqlServer
{
    /// <summary>
    /// Configuration for the SQL Server audit trail integration.
    /// </summary>
    public class SqlServerAuditorConfiguration
    {
        /// <summary>
        /// The fully-qualified (i.e. includes schema) of the table to store the audit trail in.
        /// </summary>
        public string QualifiedTableName { get; set; } = "[dbo].[AuditTrail]";
    }
}
