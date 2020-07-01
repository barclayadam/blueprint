using System;
using Blueprint.Tasks;

namespace Blueprint.Sample.WebApi.Tasks
{
    public class ConsoleWritingBackgroundTask : IBackgroundTask
    {
        public string Parameter { get; set; }

        public OkResult<object> Invoke()
        {
            Console.WriteLine("Background task passed " + Parameter);

            return new OkResult<object>("Done");
        }
    }
}
