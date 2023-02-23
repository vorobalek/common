using Microsoft.Extensions.Configuration;

namespace Common.Testing.Extensions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddTestConfiguration(this IConfigurationBuilder builder, AddTestConfiguration addTestConfiguration)
        {
            return builder.Add(new TestConfigurationSource(addTestConfiguration));
        }
    }
}