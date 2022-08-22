﻿using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;

namespace Blueprint.SqlServer;

/// <summary>
/// Provides simple extension methods to IDbConnection.
/// </summary>
public static class DbConnectionExtensions
{
    private static readonly Regex _statementSplitter = new Regex(@"\bGO\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Splits and executes a SQL block that may contain multiple statements.
    /// </summary>
    /// <param name="connection">The <strong>open</strong> database connection to use.</param>
    /// <param name="sql">The SQL that may contain mutliple statements.</param>
    [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "This is a method to be used only in test projects.")]
    public static void SplitAndExecuteSql(this IDbConnection connection, string sql)
    {
        Guard.NotNull(nameof(sql), sql);

        var commandTexts = _statementSplitter.Split(sql).Where(s => !string.IsNullOrWhiteSpace(s));

        foreach (var commandText in commandTexts)
        {
            try
            {
                connection.Execute(commandText);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute command text '{commandText}'.", ex);
            }
        }
    }
}