using System.Threading.Tasks;

namespace Common.Database.Infrastructure.Extensions
{
    public static class TaskExtensions
    {
        public static T RunSync<T>(this Task<T> task) => task.ConfigureAwait(false).GetAwaiter().GetResult();

        public static void RunSync(this Task task) => task.ConfigureAwait(false).GetAwaiter().GetResult();

        public static void RunSync(this ValueTask task) => task.ConfigureAwait(false).GetAwaiter().GetResult();
    }
}