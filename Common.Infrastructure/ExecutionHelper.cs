using System;

namespace Common.Infrastructure;

public static class ExecutionHelper
{
    public static T? TryIgnore<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception)
        {
            // ignored
        }

        return default;
    }
}