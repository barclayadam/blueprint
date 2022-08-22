namespace Blueprint.Auditing;

/// <summary>
/// An auditor presents the 'port' that allows storing <see cref="AuditItem"/>s in a
/// persistent store such as a database.
/// </summary>
/// <remarks>
/// The role of an <see cref="IAuditor"/> is to simply serialize an audit item and persist
/// it in a fashion that allows the audit trail to be queried to determine the action
/// performed within a system.
/// </remarks>
public class NullAuditor : IAuditor
{
    /// <summary>
    /// Writes an <see cref="AuditItem"/> to the persistent data store.
    /// </summary>
    /// <param name="auditItem">The audit item to persist.</param>
    public void Write(AuditItem auditItem)
    {
    }
}