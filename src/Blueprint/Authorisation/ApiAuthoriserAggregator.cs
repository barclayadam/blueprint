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
        private static readonly ConcurrentDictionary<Type, IEnumerable<IApiAuthoriser>> _operationTypeAuthorisers = new ConcurrentDictionary<Type, IEnumerable<IApiAuthoriser>>();

        private readonly IEnumerable<IApiAuthoriser> _apiAuthorisers;
        private readonly ILogger<ApiAuthoriserAggregator> _logger;

        public ApiAuthoriserAggregator(IEnumerable<IApiAuthoriser> apiAuthorisers, ILogger<ApiAuthoriserAggregator> logger)
        {
            Guard.NotNull(nameof(apiAuthorisers), apiAuthorisers);
            Guard.NotNull(nameof(logger), logger);

            this._apiAuthorisers = apiAuthorisers;
            this._logger = logger;
        }

        public async Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource)
        {
            if (this._logger.IsEnabled(LogLevel.Trace))
            {
                this._logger.LogTrace("Determining if current user can be shown link operation. type={0} resource_type={1}.", descriptor.OperationType.Name, resource.GetType().Name);
            }

            foreach (var checker in this.GetForOperation(descriptor))
            {
                var result = await checker.CanShowLinkAsync(operationContext, descriptor, resource);

                if (result.IsAllowed == false)
                {
                    // Base links could have many, potentially hundreds, of failures, which are completely
                    // normal, we will not log unless enabled
                    if (this._logger.IsEnabled(LogLevel.Trace))
                    {
                        this._logger.LogTrace("Permission check failed. reason={0} authoriser={1}", result.Reason, checker.GetType());
                    }

                    return result;
                }
            }

            if (this._logger.IsEnabled(LogLevel.Trace))
            {
                this._logger.LogTrace("Permission check succeeded");
            }

            return ExecutionAllowed.Yes;
        }

        private IEnumerable<IApiAuthoriser> GetForOperation(ApiOperationDescriptor descriptor)
        {
            return _operationTypeAuthorisers.GetOrAdd(descriptor.OperationType, t =>
            {
                return this._apiAuthorisers.Where(checker => checker.AppliesTo(descriptor)).ToList();
            });
        }
    }
}
