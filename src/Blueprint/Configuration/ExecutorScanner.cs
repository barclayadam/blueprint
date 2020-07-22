using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blueprint.Configuration
{
    /// <summary>
    /// Scans for operation executors (<see cref="IOperationExecutorBuilder" />) that handle
    /// the actual execution of registered operations.
    /// </summary>
    public class ExecutorScanner
    {
        private readonly List<IOperationExecutorBuilderScanner> scanners = new List<IOperationExecutorBuilderScanner>();

        /// <summary>
        /// Initialises a new instance of the <see cref="OperationScanner" /> class.
        /// </summary>
        public ExecutorScanner()
        {
            scanners.Add(new ApiOperationHandlerExecutorBuilderScanner());
            scanners.Add(new ApiOperationInClassConventionExecutorBuilderScanner());
        }

        /// <summary>
        /// Adds a scanner that can attempt to find executors to use for registered operations.
        /// </summary>
        /// <typeparam name="T">The scanner to add.</typeparam>
        /// <returns>This scanner.</returns>
        public ExecutorScanner AddScanner<T>() where T : IOperationExecutorBuilderScanner, new()
        {
            scanners.Add(new T());

            return this;
        }

        /// <summary>
        /// Adds a scanner that can attempt to find executors to use for registered operations.
        /// </summary>
        /// <param name="scanner">The scanner to add.</param>
        /// <returns>This scanner.</returns>
        public ExecutorScanner AddScanner(IOperationExecutorBuilderScanner scanner)
        {
            scanners.Add(scanner);

            return this;
        }

        internal void FindAndRegister(
            OperationScanner operationScanner,
            IServiceCollection services,
            List<ApiOperationDescriptor> operations)
        {
            var allFound = new List<IOperationExecutorBuilder>();

            var problems = new List<string>();

            foreach (var scanner in scanners)
            {
                foreach (var found in scanner.FindHandlers(services, operations, operationScanner.ScannedAssemblies))
                {
                    var existing = allFound.Where(e => e.Operation == found.Operation).ToList();

                    if (existing.Any())
                    {
                        var all = string.Join("\n", existing.Concat(new[] { found }).Select(e => e.ToString()));

                        problems.Add($"Multiple handlers have been found for the operation {found.Operation.Name}:\n\n{all} ");
                    }

                    allFound.Add(found);
                }
            }

            var missing = operations.Where(o => allFound.All(f => f.Operation != o)).ToList();

            if (missing.Any())
            {
                throw new MissingApiOperationHandlerException(missing.ToArray());
            }

            if (problems.Any())
            {
                throw new InvalidOperationException(string.Join("\n", problems));
            }

            services.AddSingleton(allFound);
        }
    }
}
