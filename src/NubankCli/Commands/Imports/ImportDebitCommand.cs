using SysCommand.ConsoleApp;
using System;
using NubankSharp.Services;
using NubankSharp.Extensions;
using System.IO;
using NubankSharp.Entities;
using System.Collections.Generic;
using NubankSharp.Cli.Extensions.Formatters;
using NubankSharp.Repositories.Files;

namespace NubankSharp.Cli
{
    public partial class ImportCommand : Command
    {
        public void ImportDebit(DateTime? start = null, DateTime? end = null, int agency = 0, int account = 0)
        {
            try
            {
                // 1) Obtem as transações da NuConta
                var user = this.GetCurrentUser();
                var card = new Card(user.UserName, CardType.NuConta, StatementType.ByMonth, agency, account);
                var nuApi = this.CreateNuApiByUser(user, nameof(ImportDebit));
                var transations = nuApi.GetDebitTransactions(start, end);

                // 2) Converte em extratos mensais para salvar um arquivo por mês
                var statementFileRepository = new StatementFileRepository();
                var statements = transations.ToStatementByMonth(card);
                foreach (var s in statements)
                    statementFileRepository.Save(s, this.GetStatementFileName(s));

                // 3) OUTPUT das transações
                var allSummary = transations.Summary();

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
                App.Console.Warning("Diferenças no saldo importado pode ocorrer devido aos juros adicionais que não estão no extrato da sua NuConta");
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }
    }
}
