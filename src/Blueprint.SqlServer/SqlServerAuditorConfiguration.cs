namespace Blueprint.SqlServer
{
    public class SqlServerAuditorConfiguration
    {
        public string QualifiedTableName { get; set; } = "[dbo].[AuditTrail]";
    }
}
