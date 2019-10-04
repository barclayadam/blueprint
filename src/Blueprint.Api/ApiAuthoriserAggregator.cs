using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Api.Authorisation;
using Blueprint.Core;
using Microsoft.Extensions.Logging;

namespace Blueprint.Api
{
    public class ApiAuthoriserAggregator : IApiAuthoriserAggregator
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<IApiAuthoriser>> OperationTypeAuthorisers = new ConcurrentDictionary<Type, IEnumerable<IApiAuthoriser>>();

        private readonly IEnumerable<IApiAuthoriser> apiAuthorisers;
        private readonly ILogger<ApiAuthoriserAggregator> logger;

        public ApiAuthoriserAggregator(IEnumerable<IApiAuthoriser> apiAuthorisers, ILogger<ApiAuthoriserAggregator> logger)
        {
            Guard.NotNull(nameof(apiAuthorisers), apiAuthorisers);
            Guard.NotNull(nameof(logger), logger);

            this.apiAuthorisers = apiAuthorisers;
            this.logger = logger;
        }

        public async Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Determining if current user can be shown link operation. type={0} resource_type={1}.", descriptor.OperationType.Name, resource.GetType().Name);
            }

            foreach (var checker in GetForOperation(descriptor))
            {
                var result = await checker.CanShowLinkAsync(operationContext, descriptor, resource);

                if (result.IsAllowed == false)
                {
                    // Base links could have many, potentially hundreds, of failures, which are completely
                    // normal, we will not log unless enabled
                    if (logger.IsEnabled(LogLevel.Trace))
                    {
                        logger.LogTrace("Permission check failed. reason={0} authoriser={1}", result.Reason, checker.GetType());
                    }

                    return result;
                }
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Permission check succeeded");
            }

            return ExecutionAllowed.Yes;
        }

        private IEnumerable<IApiAuthoriser> GetForOperation(ApiOperationDescriptor descriptor)
        {
            return OperationTypeAuthorisers.GetOrAdd(descriptor.OperationType, t =>
            {
                return apiAuthorisers.Where(checker => checker.AppliesTo(descriptor)).ToList();
            });
        }
    }
}
