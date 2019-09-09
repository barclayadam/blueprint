using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Api.Authorisation;
using Blueprint.Core;
using NLog;

namespace Blueprint.Api
{
    public class ApiAuthoriserAggregator : IApiAuthoriserAggregator
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentDictionary<Type, IEnumerable<IApiAuthoriser>> OperationTypeAuthorisers = new ConcurrentDictionary<Type, IEnumerable<IApiAuthoriser>>();

        private readonly IEnumerable<IApiAuthoriser> apiAuthorisers;

        public ApiAuthoriserAggregator(IEnumerable<IApiAuthoriser> apiAuthorisers)
        {
            Guard.NotNull(nameof(apiAuthorisers), apiAuthorisers);

            this.apiAuthorisers = apiAuthorisers;
        }

        public async Task<ExecutionAllowed> CanShowLinkAsync(ApiOperationContext operationContext, ApiOperationDescriptor descriptor, object resource)
        {
            if (Log.IsTraceEnabled)
            {
                Log.Trace("Determining if current user can be shown link operation. type={0} resource_type={1}.", descriptor.OperationType.Name, resource.GetType().Name);
            }

            foreach (var checker in GetForOperation(descriptor))
            {
                var result = await checker.CanShowLinkAsync(operationContext, descriptor, resource);

                if (result.IsAllowed == false)
                {
                    // Base links could have many, potentially hundreds, of failures, which are completely
                    // normal, we will not log unless enabled
                    if (Log.IsTraceEnabled)
                    {
                        Log.Trace("Permission check failed. reason={0} authoriser={1}", result.Reason, checker.GetType());
                    }

                    return result;
                }
            }

            if (Log.IsTraceEnabled)
            {
                Log.Trace("Permission check succeeded");
            }

            return ExecutionAllowed.Yes;
        }

        private IEnumerable<IApiAuthoriser> GetForOperation(ApiOperationDescriptor descriptor)
        {
            return OperationTypeAuthorisers.GetOrAdd(descriptor.OperationType, t =>
            {
                return this.apiAuthorisers.Where(checker => checker.AppliesTo(descriptor)).ToList();
            });
        }
    }
}
