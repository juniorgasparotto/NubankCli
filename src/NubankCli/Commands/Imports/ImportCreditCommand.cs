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
        public void ImportCredit(DateTime? start = null, DateTime? end = null, StatementType statementType = StatementType.ByBill)
        {
            try
            {
                var user = this.GetCurrentUser();
                var card = new Card(user, CardType.CreditCard, statementType);
                var repository = this.GetNubankRepositoryByUser(user);
                var billService = this.GetService<BillService>();
                var statementService = this.GetService<StatementService>();
                var jsonManager = this.GetService<JsonFileManager>();

                var events = repository.GetEvents();

                List<Statement> statements;

                if (statementType == StatementType.ByMonth)
                {
                    var billsTransactions = billService.GetBillItemsByMonth(start, end);
                    billService.PopulateEvents(billsTransactions, events);
                    statements = statementService.ToStatementByMonth(billsTransactions, card);
                }
                else
                {
                    var bills = billService.GetBills(start, end);
                    billService.PopulateEvents(bills, events);
                    statements = statementService.ToStatementByBill(bills, card);
                }

                foreach (var e in statements)
                    jsonManager.Save(e, e.GetPath());

                statements = statements.ExcludeBillPaymentLastBill();

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
                App.Console.Warning("Transações do tipo 'Pagamento Recebido' foram importadas, mas não estão sendo consideradas nas somatórias para dar coerência com os totais de cada boleto");
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }
    }
}
