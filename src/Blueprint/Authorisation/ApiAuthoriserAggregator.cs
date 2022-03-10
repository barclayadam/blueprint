using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Blueprint.Authorisation
{
    public class ApiAuthoriserAggregator : IApiAuthoriserAggregator
    {
        private readonly IEnumerable<IApiAuthoriser> _apiAuthorisers;
        private readonly ILogger<ApiAuthoriserAggregator> _logger;

        public ApiAuthoriserAggregator(IEnumerable<IApiAuthoriser> apiAuthorisers, ILogger<ApiAuthoriserAggregator> logger)
        {
            Guard.NotNull(nameof(apiAuthorisers), apiAuthorisers);
            Guard.NotNull(nameof(logger), logger);

            this._apiAuthorisers = apiAuthorisers;
            this._logger = logger;
        }

        public async ValueTask<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, [CanBeNull] object resource)
        {
            var traceLogEnabled = this._logger.IsEnabled(LogLevel.Trace);

            foreach (var checker in this._apiAuthorisers)
            {
                if (!checker.AppliesTo(descriptor))
                {
                    continue;
                }

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
                            resource?.GetType().Name,
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
                    resource?.GetType().Name);
            }

            return ExecutionAllowed.Yes;
        }
    }
}
