using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Configuration;

/// <summary>
/// Scans for operation executors (<see cref="IOperationExecutorBuilder" />) that handle
/// the actual execution of registered operations.
/// </summary>
public class ExecutorScanner
{
    private readonly List<IOperationExecutorBuilderScanner> _scanners = new ();

    /// <summary>
    /// Initialises a new instance of the <see cref="OperationScanner" /> class.
    /// </summary>
    public ExecutorScanner()
    {
        this._scanners.Add(new ApiOperationHandlerExecutorBuilderScanner());
        this._scanners.Add(new ApiOperationInClassConventionExecutorBuilderScanner());
    }

    /// <summary>
    /// Adds a scanner that can attempt to find executors to use for registered operations.
    /// </summary>
    /// <typeparam name="T">The scanner to add.</typeparam>
    /// <returns>This scanner.</returns>
    public ExecutorScanner AddScanner<T>() where T : IOperationExecutorBuilderScanner, new()
    {
        return this.AddScanner(new T());
    }

    /// <summary>
    /// Adds a scanner that can attempt to find executors to use for registered operations.
    /// </summary>
    /// <param name="scanner">The scanner to add.</param>
    /// <returns>This scanner.</returns>
    public ExecutorScanner AddScanner(IOperationExecutorBuilderScanner scanner)
    {
        this._scanners.Add(scanner);

        return this;
    }

    internal void FindAndRegister(
        OperationScanner operationScanner,
        IServiceCollection services,
        List<ApiOperationDescriptor> operations)
    {
        var problems = new List<string>();

        foreach (var operation in operations)
        {
            foreach (var scanner in this._scanners)
            {
                var handlers = scanner.FindHandlers(services, operation.OperationType, operationScanner.ScannedAssemblies);

                foreach (var handler in handlers)
                {
                    // Not ideal using exceptions here, done to avoid duplicating the exception messaging around multiple operations
                    try
                    {
                        operation.RegisterHandler(handler);
                    }
                    catch (Exception e)
                    {
                        problems.Add(e.Message);
                    }
                }
            }

            if (operation.Handlers.Count == 0)
            {
                problems.Add($"Could not find any handlers for the operation {operation}");
            }
        }

        if (problems.Any())
        {
            throw new InvalidOperationException($"Problems were found during handler scanning:\n\n{string.Join("\n", problems)}");
        }
    }
}