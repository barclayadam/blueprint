using System.Threading.Tasks;

namespace Blueprint.Utilities
{
    public static class TaskExtensions
    {
        public static Task<object> ConvertToObject<T>(this Task<T> task)
        {
            var res = new TaskCompletionSource<object>();

            return task.ContinueWith(
                t =>
                {
                    if (t.IsCanceled)
                    {
                        res.TrySetCanceled();
                    }
                    else if (t.IsFaulted)
                    {
                        res.TrySetException(t.Exception);
                    }
                    else
                    {
                        res.TrySetResult(t.Result);
                    }

                    return res.Task;
                }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }
    }
}
