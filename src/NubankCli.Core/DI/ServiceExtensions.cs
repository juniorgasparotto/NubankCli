using Microsoft.Extensions.DependencyInjection;
using NubankCli.Core.Services;

namespace NubankCli.Core.DI
{
    public static class ServiceExtensions
    {

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Deve ser compartilhado em todos os lugares
            services.AddScoped<BillService>();
            services.AddScoped<SavingService>();            
            services.AddScoped<StatementService>();
            return services;
        }
    }
}
