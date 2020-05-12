using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Sample.WebApi.Tasks;
using Blueprint.Tasks;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("background-task")]
    public class AddBackgroundTaskCommand : ICommand
    {
        [Required]
        public string Parameter { get; set; }

        public OkResult<object> Invoke(IBackgroundTaskScheduler taskScheduler)
        {
            taskScheduler.Enqueue(new ConsoleWritingBackgroundTask {Parameter = Parameter});

            return new OkResult<object>(new
            {
                Scheduled = true,
            });
        }
    }
}
