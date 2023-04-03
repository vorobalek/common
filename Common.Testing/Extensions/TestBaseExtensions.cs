using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Testing.Extensions;

public static class TestBaseExtensions
{
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
            await testBase.PrepareAsync(cancellationToken).ConfigureAwait(false);
            if (configuration.PrepareAsync != default)
                await configuration.PrepareAsync(context, cancellationToken).ConfigureAwait(false);
            await actionAsync(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            if (configuration.HandleExceptionAsync != default)
                await configuration.HandleExceptionAsync(context, exception, cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally
        {
            if (configuration.FinalizeAsync != default)
                await configuration.FinalizeAsync(context, cancellationToken).ConfigureAwait(false);
            await testBase.FinalizeAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}