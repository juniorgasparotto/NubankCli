using Microsoft.Extensions.DependencyInjection;
using NubankSharp.Commands.Statements;
using NubankSharp.Commands.Transactions;
using NubankSharp.Extensions.Tables;
using NubankSharp.Entities;

namespace NubankSharp.Extensions.Tables
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
