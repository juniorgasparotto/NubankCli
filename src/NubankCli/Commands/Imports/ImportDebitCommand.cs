using SysCommand.ConsoleApp;
using System;
using NubankCli.Core;
using NubankCli.Core.Services;
using NubankCli.Extensions;
using System.IO;
using NubankCli.Core.Entities;
using System.Collections.Generic;
using NubankCli.Core.Extensions;
using NubankCli.Core.Extensions.Formatters;

namespace NubankCli.Cli
{
    public partial class ImportCommand : Command
    {
        public void ImportDebit(DateTime? start = null, DateTime? end = null, int agency = 0, int account = 0)
        {
            try
            {
                var user = this.GetCurrentUser();
                var card = new Card(user, CardType.NuConta, StatementType.ByMonth, agency, account);
                var repository = this.GetNubankRepositoryByUser(user);
                var savingsService = this.GetService<SavingService>();
                var statementService = this.GetService<StatementService>();
                var jsonManager = this.GetService<JsonFileManager>();

                List<Statement> statements;
                var savings = savingsService.GetItemsByMonth(start, end);

                statements = statementService.ToStatementByMonth(savings, card);

                foreach (var e in statements)
                    jsonManager.Save(e, e.GetPath());

                var allTransactions = statements.GetTransactions();
                var allSummary = allTransactions.Summary();

                App.Console.Success($" ");
                App.Console.Success($"TOTAL (ENTRADA): " + $"{allSummary.ValueIn.Format()} ({allSummary.CountIn})");
                App.Console.Success($"TOTAL (SAÍDA)  : " + $"{allSummary.ValueOut.Format()} ({allSummary.CountOut})");
                App.Console.Success($"TOTAL          : " + $"{allSummary.ValueTotal.Format()} ({allSummary.CountTotal})");

                App.Console.Write($" ");
                this.ViewFormatted(statements);

                if (statements.Count > 0)
                {
                    App.Console.Warning($" ");
                    App.Console.Warning($"TRANSAÇÕES IMPORTADAS EM:");
                    App.Console.Warning($"    {Path.GetFullPath(card.GetPath())}");
                }

                App.Console.Warning($" ");
                App.Console.Warning("Diferenças no saldo importado pode ocorrer devido aos juros adicionais que não estão no extrato da sua NuConta");
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }
    }
}
