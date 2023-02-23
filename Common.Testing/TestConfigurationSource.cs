using Microsoft.Extensions.Configuration;

namespace Common.Testing;

public class TestConfigurationSource : IConfigurationSource
{
    private readonly AddTestConfiguration _addTestConfiguration;

    public TestConfigurationSource(AddTestConfiguration addTestConfiguration)
    {
        _addTestConfiguration = addTestConfiguration;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new TestConfigurationProvider(_addTestConfiguration);
    }
}