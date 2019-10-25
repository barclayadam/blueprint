using System.Threading.Tasks;

namespace Blueprint.Api.Authorisation
{
    public struct ExecutionAllowed
    {
        public static readonly ExecutionAllowed Yes = new ExecutionAllowed
        {
            IsAllowed = true,
        };

        public static readonly Task<ExecutionAllowed> YesTask = Task.FromResult(Yes);

        public bool IsAllowed { get; private set; }

        public string Reason { get; private set; }

        public string Message { get; private set; }

        public ExecutionAllowedFailureType? FailureType { get; private set; }

        public static ExecutionAllowed No(string reason, string message, ExecutionAllowedFailureType failureType)
        {
            return new ExecutionAllowed
            {
                IsAllowed = false,
                Reason = reason,
                Message = message,
                FailureType = failureType,
            };
        }
    }
}
