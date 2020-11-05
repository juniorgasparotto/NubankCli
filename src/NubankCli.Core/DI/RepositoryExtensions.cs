using Microsoft.Extensions.DependencyInjection;
using NubankCli.Core.Repositories;
using NubankCli.Core.Repositories.Api;

namespace NubankCli.Core.DI
{
    public static class RepositoryExtensions
    {

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Deve ser compartilhado em todos os lugares
            services.AddScoped<NubankRepository>();
            services.AddScoped<EndpointsRepository>();
            services.AddScoped<RestSharpClient>();
            services.AddScoped<StatementRepository>();
            
            return services;
        }
    }
}
