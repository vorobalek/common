using System.Threading;
using System.Threading.Tasks;

namespace Common.Infrastructure.Extensions;

public static class TaskExtensions
{
    public static T RunSync<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RunSync(this Task task)
    {
        task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void RunSync(this ValueTask task)
    {
        task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static void DontWait(this Task task, CancellationToken cancellationToken = default)
    {
        ExecutionHelper.TryIgnore(() => Task.Run(async () => await task, cancellationToken));
    }

    public static void DontWait(this ValueTask task, CancellationToken cancellationToken = default)
    {
        ExecutionHelper.TryIgnore(() => Task.Run(async () => await task, cancellationToken));
    }
}