using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Testing.Extensions;

public static class TestBaseExtensions
{
    public static Dictionary<string, string?> ResetValues(this Dictionary<string, string?> dictionary,
        params string[] keys)
    {
        foreach (var key in keys) dictionary.ReplaceValue(key, null);
        return dictionary;
    }

    public static Dictionary<string, string?> ReplaceValues(this Dictionary<string, string?> dictionary,
        Dictionary<string, string?> replaceMap)
    {
        foreach (var (key, value) in replaceMap) dictionary.ReplaceValue(key, value);
        return dictionary;
    }

    public static Dictionary<string, string?> ReplaceValue(this Dictionary<string, string?> dictionary, string key,
        string? value)
    {
        if (!dictionary.ContainsKey(key))
            dictionary.Add(key, value);
        else
            dictionary[key] = value;
        return dictionary;
    }

    public static async Task IsolatedAsync<TStartup>(
        this TestBase<TStartup> testBase,
        IsolatedActionAsyncDelegate actionAsync,
        ConfigureIsolatedContextDelegate? configure = default,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string? methodName = null)
        where TStartup : class
    {
        var configuration = new IsolatedContextConfiguration();
        configure?.Invoke(configuration);

        var context = new IsolatedContext();

        try
        {
            await testBase.PrepareAsync(cancellationToken);
            if (configuration.PrepareAsync != default) await configuration.PrepareAsync(context, cancellationToken);
            await actionAsync(context, cancellationToken);
        }
        catch (Exception exception)
        {
            if (configuration.HandleExceptionAsync != default)
                await configuration.HandleExceptionAsync(context, exception, cancellationToken);
            throw;
        }
        finally
        {
            if (configuration.FinalizeAsync != default) await configuration.FinalizeAsync(context, cancellationToken);
            await testBase.FinalizeAsync(cancellationToken);
        }
    }
}