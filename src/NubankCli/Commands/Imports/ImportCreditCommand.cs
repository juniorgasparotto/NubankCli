using SysCommand.ConsoleApp;
using System;
using NubankSharp.Services;
using NubankSharp.Extensions;
using System.IO;
using NubankSharp.Entities;
using System.Collections.Generic;
using NubankSharp.Cli.Extensions.Formatters;
using System.Linq;
using NubankSharp.Repositories.Files;
using NubankSharp.Repositories.Api.Services;

namespace NubankSharp.Cli
{
    public partial class ImportCommand : Command
    {
        public void ImportCredit(DateTime? start = null, DateTime? end = null, StatementType statementType = StatementType.ByBill)
        {
            try
            {
                var user = this.GetCurrentUser();
                var card = new Card(user.UserName, CardType.CreditCard, statementType);
                var nuApi = this.CreateNuApiByUser(user, nameof(ImportCredit));

                // 1) Obtem os extratos separado de forma mensal ou como boletos
                List<Statement> statements;
                if (statementType == StatementType.ByMonth)
                {
                    var transactions = nuApi.GetCreditTransactions(start, end);
                    statements = transactions.ToStatementByMonth(card);
                }
                else
                {
                    var bills = nuApi.GetBills(start, end);
                    statements = bills.ToStatementByBill(card);
                }

                // 2) Converte em extratos mensais para salvar um arquivo por mês
                var statementFileRepository = new StatementFileRepository();
                foreach (var e in statements)
                    statementFileRepository.Save(e, this.GetStatementFileName(e));

                statements = statements.ExcludeBillPaymentLastBill();

                // 3) OUTPUT das transações
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
                    App.Console.Warning($"    {Path.GetFullPath(this.GetCardPath(card))}");
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
