namespace NubankCli.Commands.Statements
{
    using SysCommand.ConsoleApp;
    using SysCommand.Mapping;
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using NubankCli.Core.Repositories;
    using NubankCli.Extensions;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using NubankCli.Core.Extensions.Formatters;
    using NubankCli.Core.Extensions;
    using NubankCli.Core;
    using NubankCli.Core.Services;
    using NubankCli.Core.Entities;
    using System.Collections.Generic;

    public partial class StatementCommand : Command
    {

        [Action(Name = "get")]
        public void Get(
           EntityNames type,
           
           [Argument(ShortName = 'c', LongName = "card")] CardType? card = null,
           [Argument(ShortName = 'm', LongName = "merge")] bool merge = false,
           [Argument(ShortName = 'h', LongName = "by-month")] bool byMonth = false,
           [Argument(ShortName = 'e', LongName = "exclude-redundance")] bool excludeRedundance = true,
           [Argument(ShortName = 'w', LongName = "where")] string where = null,
           [Argument(ShortName = 's', LongName = "sort")] string sort = "Start ASC",
           [Argument(ShortName = 'P', LongName = "page")] int page = Constants.FIRST_PAGE,
           [Argument(ShortName = 'S', LongName = "page-size")] int pageSize = 100,
           [Argument(ShortName = 'A', LongName = "auto-page")] bool autoPage = true,
           [Argument(ShortName = 'o', LongName = "output")] string outputFormat = null
        )
        {
            try
            {
                var repository = this.GetService<StatementRepository>();
                IEnumerable<Statement> statements;

                // 1) Quando for cartão de credito, então removemos os pagamentos por padrão para que a visualização fique igual ao valor do boleto do APP
                // 2) Quando for nuConta, então mantemos entradas e saídas para o saldo ser o mais próximo possível da APP
                // 3) Quando for NULL (ambos), então removemos as transações correlacionadas, ou seja, transações que saíram da nuConta e entraram no cartão de crédito
                //    Isso deve ser feito pois do contrário teremos o saldo de entrada e saída duplicados:
                //    NuConta: 
                //      1) Entrou 100 reais do banco X para pagar o boleto
                //      2) Saiu 100 da nuConta para ir para o crédito
                //          ENTRADA: 100
                //          SAÍDA: 100
                //          SALDO: 0
                //    Crédito: 
                //      1) Entrou 100 reais para pagar o boleto
                //      2) A somatória das contas equivalem a 100 reais.
                //          ENTRADA: 100
                //          SAÍDA: 100
                //          SALDO: 0
                //    Total:
                //      1) É feito a somatória dos dois cartões e ai temos o problema da duplicação da ENTRADA e SAÍDA
                //          ENTRADA: 200
                //          SAÍDA: 200
                //          SALDO: 0
                if (card == CardType.CreditCard)
                    statements = repository.GetStatements(this.GetCurrentUser(), card, excludeRedundance, false, where, null, sort);
                else if (card == CardType.NuConta)
                    statements = repository.GetStatements(this.GetCurrentUser(), card, false, false, where, null, sort);
                else
                    statements = repository.GetStatements(this.GetCurrentUser(), card, false, excludeRedundance, where, null, sort);

                if (byMonth || merge)
                {
                    var statementService = this.GetService<StatementService>();
                    statements = statementService.ToStatementByMonth(statements.GetTransactions(), merge);
                }

                var summary = statements.GetTransactions().Summary();

                App.Console.Success($" ");
                App.Console.Success($"TOTAL (ENTRADA): " + $"{summary.ValueIn.Format()} ({summary.CountIn})");
                App.Console.Success($"TOTAL (SAÍDA)  : " + $"{summary.ValueOut.Format()} ({summary.CountOut})");
                App.Console.Success($"TOTAL          : " + $"{summary.ValueTotal.Format()} ({summary.CountTotal})");
                App.Console.Success($" ");

                if (page <= 0 || pageSize <= 0)
                {
                    this.ViewFormatted(statements, outputFormat);
                }
                else
                {
                    this.ViewPagination(page, currentPage =>
                    {
                        var pageResult = statements.AsQueryable().PageResult(currentPage, pageSize);
                        return pageResult;
                    }, autoPage, outputFormat);
                }

                if (card == CardType.CreditCard)
                {
                    App.Console.Warning($" ");
                    App.Console.Warning("Transações do tipo 'Pagamento Recebido' não estão sendo consideradas nas somatórias. Para inclui-las utilize '--exclude-redundance false' ou '-e false'");
                }
                else if (card == null)
                {
                    App.Console.Warning($" ");
                    App.Console.Warning("Transações correlacionadas entre a NuConta e o cartão de crédito foram removidas para que os saldos de entrada e saídas não sejam duplicados");
                }
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }
    }
}
