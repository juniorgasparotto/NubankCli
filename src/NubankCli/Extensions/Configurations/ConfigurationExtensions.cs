using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NubankSharp.Models;
using System.IO;

namespace NubankCli.Extensions.Configurations
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddConfigurations(this IServiceCollection services, string configFolder, string settingsFileName)
        {
            var builder = new ConfigurationBuilder().AddJsonFile(Path.Combine(configFolder, settingsFileName));

            var config = builder.Build();
            services.AddScoped<IConfiguration>((s) => config);

            services.Configure<NuAppSettings>(options => config.Bind(options));
            return services;
        }
    }
}
