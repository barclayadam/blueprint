using System.Threading.Tasks;

namespace Blueprint.Api.Authorisation
{
    public struct ExecutionAllowed
    {
        public static readonly ExecutionAllowed Yes = new ExecutionAllowed(true, null, null, null);

        public static readonly Task<ExecutionAllowed> YesTask = Task.FromResult(Yes);

        private ExecutionAllowed(bool isAllowed, string reason, string message, ExecutionAllowedFailureType? failureType)
        {
            IsAllowed = isAllowed;
            Reason = reason;
            Message = message;
            FailureType = failureType;
        }

        public bool IsAllowed { get; }

        public string Reason { get; }

        public string Message { get; }

        public ExecutionAllowedFailureType? FailureType { get; }

        public static ExecutionAllowed No(string reason, string message, ExecutionAllowedFailureType failureType)
        {
            return new ExecutionAllowed(false, reason, message, failureType);
        }
    }
}