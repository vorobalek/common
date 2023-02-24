using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Infrastructure;
using Common.Testing.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Common.Testing;

public delegate void AddTestConfiguration(Dictionary<string, string?> configuration);

public abstract class TestBase<TStartup> :
    IClassFixture<WebApplicationFactory<TStartup>>
    where TStartup : class
{
    private readonly WebApplicationFactory<TStartup> _sharedFactory;
    private AddTestConfiguration _addTestConfiguration = _ => { };
    private HttpClient? _client;
    private WebApplicationFactory<TStartup>? _factory;
    private string? _pathToSolutionRoot;
    private IServiceProvider? _serviceProvider;
    private IServiceScope? _serviceScope;

    protected TestBase(WebApplicationFactory<TStartup> sharedFactory)
    {
        _sharedFactory = sharedFactory;
    }

    private IServiceProvider ServiceProvider => _serviceProvider ??= CreateScopedServiceProvider();

    private TestServer Server
    {
        get
        {
            EnsureInitialized();
            return _factory!.Server;
        }
    }

    protected bool IsInitialized { get; private set; }
    protected virtual string? BaseUrl => null;
    protected string PathToSolutionRoot => _pathToSolutionRoot ??= FindSolutionRoot();
    protected abstract string PathToContentRoot { get; }

    protected TService GetService<TService>() where TService : notnull
    {
        return ServiceProvider.GetRequiredService<TService>();
    }

    protected IEnumerable<TService> GetServices<TService>()
    {
        return ServiceProvider.GetRequiredService<IEnumerable<TService>>();
    }

    protected TOptions GetOptions<TOptions>() where TOptions : class, new()
    {
        return GetService<IOptions<TOptions>>().Value;
    }

    public IServiceProvider CreateScopedServiceProvider()
    {
        _serviceScope = Server.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        return _serviceScope.ServiceProvider;
    }

    private void EnsureInitialized()
    {
        if (IsInitialized) return;

        _factory = CreateFactory(_sharedFactory);
        _client = CreateClient(_factory);

        IsInitialized = true;
    }

    private WebApplicationFactory<TStartup> CreateFactory(WebApplicationFactory<TStartup> factory)
    {
        ExecutionEnvironment.SetTesting(true);
        return factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Local");
            builder.UseContentRoot(Path.Combine(PathToSolutionRoot, PathToContentRoot));
            builder.ConfigureServices(ConfigureServices);
            builder.ConfigureTestServices(ConfigureTestServices);
            builder.ConfigureAppConfiguration(ConfigureAppConfiguration);
        });
    }

    private string FindSolutionRoot()
    {
        if (!string.IsNullOrWhiteSpace(_pathToSolutionRoot)) return _pathToSolutionRoot;

        const string solutionName = "*.sln";
        var directoryInfo = new DirectoryInfo(AppContext.BaseDirectory);
        while (Directory.EnumerateFiles(directoryInfo.FullName, solutionName).FirstOrDefault() == null)
            directoryInfo =
                directoryInfo.Parent
                ?? throw new InvalidOperationException(
                    $"Solution root could not be located using application root {AppContext.BaseDirectory}.");

        return Path.GetFullPath(directoryInfo.FullName);
    }

    protected virtual void ConfigureTestServices(IServiceCollection collection)
    {
    }


    protected virtual void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
    {
    }

    protected virtual void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder builder)
    {
        builder.AddTestConfiguration(_addTestConfiguration);
    }

    private HttpClient CreateClient(WebApplicationFactory<TStartup> factory)
    {
        var options = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        };

        if (string.IsNullOrWhiteSpace(BaseUrl))
            options.BaseAddress = new Uri(options.BaseAddress, BaseUrl);

        return factory.CreateClient(options);
    }

    public virtual Task PrepareAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task FinalizeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void AddTestConfiguration(AddTestConfiguration action)
    {
        _addTestConfiguration += action;
    }
}