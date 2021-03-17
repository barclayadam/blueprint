using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Blueprint.Authorisation
{
    public class ApiAuthoriserAggregator : IApiAuthoriserAggregator
    {
        private static readonly ConcurrentDictionary<Type, List<IApiAuthoriser>> _operationTypeAuthorisers = new ConcurrentDictionary<Type, List<IApiAuthoriser>>();

        private readonly IEnumerable<IApiAuthoriser> _apiAuthorisers;
        private readonly ILogger<ApiAuthoriserAggregator> _logger;

        public ApiAuthoriserAggregator(IEnumerable<IApiAuthoriser> apiAuthorisers, ILogger<ApiAuthoriserAggregator> logger)
        {
            Guard.NotNull(nameof(apiAuthorisers), apiAuthorisers);
            Guard.NotNull(nameof(logger), logger);

            this._apiAuthorisers = apiAuthorisers;
            this._logger = logger;
        }

        public async ValueTask<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource)
        {
            var traceLogEnabled = this._logger.IsEnabled(LogLevel.Trace);

            foreach (var checker in this.GetForOperation(descriptor))
            {
                var result = await checker.CanShowLinkAsync(operationContext, descriptor, resource);

                if (result.IsAllowed == false)
                {
                    // Base links could have many, potentially hundreds, of failures, which are completely
                    // normal, we will not log unless enabled
                    if (traceLogEnabled)
                    {
                        this._logger.LogTrace(
                            "Link check failed. type={0} resource_type={1} reason={2} authoriser={3}",
                            descriptor.OperationType.Name,
                            resource.GetType().Name,
                            result.Reason,
                            checker.GetType());
                    }

                    return result;
                }
            }

            if (traceLogEnabled)
            {
                this._logger.LogTrace(
                    "Link check succeeded. type={0} resource_type={1}",
                    descriptor.OperationType.Name,
                    resource.GetType().Name);
            }

            return ExecutionAllowed.Yes;
        }

        private List<IApiAuthoriser> GetForOperation(ApiOperationDescriptor descriptor)
        {
            return _operationTypeAuthorisers.GetOrAdd(descriptor.OperationType, t =>
            {
                return this._apiAuthorisers.Where(checker => checker.AppliesTo(descriptor)).ToList();
            });
        }
    }
}
