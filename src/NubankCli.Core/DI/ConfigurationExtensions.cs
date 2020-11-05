using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NubankCli.Core.Configuration;
using System.IO;

namespace NubankCli.Core.DI
{
    public static class ConfigurationExtensions
    {

        public static IServiceCollection AddConfiguration(this IServiceCollection services, string configFolder, string settingsFileName)
        {
            var builder = new ConfigurationBuilder().AddJsonFile(Path.Combine(configFolder, settingsFileName));

            var config = builder.Build();
            services.AddScoped<IConfiguration>((s) => config);
            services.Configure<AppSettings>(options => config.Bind(options));

            return services;
        }
    }
}
