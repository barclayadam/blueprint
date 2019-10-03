using Blueprint.Api;
using Microsoft.Extensions.Logging;

namespace Blueprint.Sample.Console.CounterApp.Api
{
    [RootLink("incrementCount")]
    public class IncrementCountCommand : ICommand
    {
        public int Max { get; set; }
    }

    public class IncrementCountCommandHandler : SyncApiOperationHandler<IncrementCountCommand>
    {
        private readonly ILogger logger;

        // This exists here for simplicity
        public static int Counter;

        public IncrementCountCommandHandler(ILogger<IncrementCountCommandHandler> logger)
        {
            this.logger = logger;
        }

        public override object InvokeSync(IncrementCountCommand operation, ApiOperationContext apiOperationContext)
        {
            if (operation.Max != -1 && Counter >= operation.Max)
            {
                logger.LogWarning("Reached max count");

                return null;
            }

            Counter++;

            logger.LogInformation("Counter is {0}", Counter);

            return null;
        }
    }
}
