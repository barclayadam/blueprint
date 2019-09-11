using Blueprint.Api;
using NLog;

namespace Blueprint.Sample.Console.CounterApp.Api
{
    [RootLink("incrementCount")]
    public class IncrementCountCommand : ICommand
    {
        public int Max { get; set; }
    }

    public class IncrementCountCommandHandler : SyncApiOperationHandler<IncrementCountCommand>
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // This exists here for simplicity
        public static int Counter;

        public override object InvokeSync(IncrementCountCommand operation, ApiOperationContext apiOperationContext)
        {
            if (operation.Max != -1 && Counter >= operation.Max)
            {
                Log.Warn("Reached max count");

                return null;
            }

            Counter++;

            System.Console.WriteLine($"Counter: {Counter}");

            return null;
        }
    }
}
