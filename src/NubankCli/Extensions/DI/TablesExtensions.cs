using Microsoft.Extensions.DependencyInjection;
using NubankCli.Commands.Statements;
using NubankCli.Commands.Transactions;
using NubankCli.Extensions.Tables;
using NubankCli.Core.Entities;

namespace NubankCli.Extensions.DI
{
    public static class TablesExtensions
    {
        public static IServiceCollection ConfigureTables(this IServiceCollection service)
        {
            service.AddSingleton<ITableConfig<Transaction>>(s => new TransactionTableConfig());
            service.AddSingleton<ITableConfig<Statement>>(s => new StatementTableConfig());
            
            return service;
        }
    }
}
