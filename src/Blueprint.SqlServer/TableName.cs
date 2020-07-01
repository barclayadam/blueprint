using System;
using System.Text.RegularExpressions;

namespace Blueprint.SqlServer
{
    internal class TableName : IEquatable<TableName>
    {
        private const string DefaultSchema = "dbo";

        /// <summary>
        /// Creates a <see cref="TableName" /> object with the given schema and table names.
        /// </summary>
        /// <param name="schema">The database schema for the table.</param>
        /// <param name="tableName">The table name.</param>
        public TableName(string schema, string tableName)
        {
            Guard.NotNullOrEmpty(nameof(schema), schema);
            Guard.NotNullOrEmpty(nameof(tableName), tableName);

            Schema = StripBrackets(schema);
            Name = StripBrackets(tableName);
        }

        /// <summary>Gets the schema name of the table.</summary>
        public string Schema { get; }

        /// <summary>Gets the table's name.</summary>
        public string Name { get; }

        internal string QualifiedTableName => "[" + Schema + "].[" + Name + "]";

        /// <summary>
        /// Checks whether the two <see cref="TableName" /> objects are equal (i.e. represent the same table).
        /// </summary>
        /// <param name="left">The table name on the LHS of the operator.</param>
        /// <param name="right">The table name on the RHS of the operator.</param>
        public static bool operator ==(TableName left, TableName right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Checks whether the two <see cref="TableName" /> objects are not equal (i.e. do not represent the same table).
        /// </summary>
        /// <param name="left">The table name on the LHS of the operator.</param>
        /// <param name="right">The table name on the RHS of the operator.</param>
        public static bool operator !=(TableName left, TableName right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Parses the given name into a <see cref="TableName" />, defaulting to using the 'dbo' schema unless the name is schema-qualified.
        /// E.g. 'table' will result in a <see cref="TableName" /> representing the '[dbo].[table]' table, whereas 'accounting.messages' will
        /// represent the '[accounting].[messages]' table.
        /// </summary>
        /// <param name="name">The table name to parse.</param>
        /// <returns>A newly parsed <see cref="TableName"/>.</returns>
        public static TableName Parse(string name)
        {
            if (!name.StartsWith("[") || !name.EndsWith("]"))
            {
                var parts = name.Split('.');

                return TableNameFromParts(name, parts);
            }

            var parts1 = Regex.Split(name.Substring(1, name.Length - 2), "\\][ ]*\\.[ ]*\\[");

            return TableNameFromParts(name, parts1);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return QualifiedTableName;
        }

        /// <inheritdoc />
        public bool Equals(TableName other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Schema, other.Schema, StringComparison.OrdinalIgnoreCase) && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((TableName)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Schema.GetHashCode() * 397 ^ Name.GetHashCode();
        }

        private static TableName TableNameFromParts(string name, string[] parts)
        {
            switch (parts.Length)
            {
                case 1:
                    return new TableName(DefaultSchema, parts[0]);
                case 2:
                    return new TableName(parts[0], parts[1]);
                default:
                    throw new ArgumentException("The table name '" + name + "' cannot be used because it contained multiple '.' characters - if you intend to use '.' as part of a table name, please be sure to enclose the name in brackets, e.g. like this: '[Table name with spaces and .s]'");
            }
        }

        private static string StripBrackets(string value)
        {
            if (value.StartsWith("["))
            {
                value = value.Substring(1);
            }

            if (value.EndsWith("]"))
            {
                value = value.Substring(0, value.Length - 1);
            }

            return value;
        }
    }
}
