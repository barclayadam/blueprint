using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Blueprint.Core.Apm;

namespace Blueprint.Apm.Stackify
{
    public class StackifyApmTool : IApmTool
    {
        public Task InvokeAsync(string operationName, Func<Task> executor)
        {
            return StackifyLib.ProfileTracer
                .CreateAsOperation(operationName, Activity.Current.Id)
                .ExecAsync(executor);
        }

        public Task TrackDependencyAsync(string operationName, string target, string type, string extraData, Func<IApmDependencyOperation, Task> executor)
        {
            return StackifyLib.ProfileTracer
                .CreateAsDependency($"${operationName} @ {target}", type)
                .ExecAsync(() => executor(NulloApmDependencyOperation.Instance));
        }
    }
}
