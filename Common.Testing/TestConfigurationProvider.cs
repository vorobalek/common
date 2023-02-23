using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Common.Testing;

public class TestConfigurationProvider : ConfigurationProvider
{
    private readonly AddTestConfiguration _addTestConfiguration;

    public TestConfigurationProvider(AddTestConfiguration addTestConfiguration)
    {
        _addTestConfiguration = addTestConfiguration;
    }

    public override void Load()
    {
        var configuration = new Dictionary<string, string?>();
        _addTestConfiguration(configuration);
        Data = configuration;
    }
}