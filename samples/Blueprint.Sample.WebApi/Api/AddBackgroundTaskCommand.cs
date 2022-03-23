using System.ComponentModel.DataAnnotations;
using Blueprint.Authorisation;
using Blueprint.Sample.WebApi.Tasks;
using Blueprint.Tasks;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("background-task")]
    [AllowAnonymous]
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
