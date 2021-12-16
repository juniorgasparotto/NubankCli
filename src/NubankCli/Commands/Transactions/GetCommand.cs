﻿namespace NubankSharp.Commands.Transactions
{
    using SysCommand.ConsoleApp;
    using SysCommand.Mapping;
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using NubankSharp.Extensions;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using NubankSharp.Cli.Extensions.Formatters;
    using NubankSharp.Repositories.Files;

    public partial class TransactionCommand : Command
    {

        [Action(Name = "get")]
        public void Get(
           EntityNames type,
           [Argument(ShortName = 'i', LongName = "id-or-name")] string id = null,
           [Argument(ShortName = 'w', LongName = "where")] string where = null,
           [Argument(ShortName = 's', LongName = "sort")] string sort = "EventDate DESC",
           [Argument(ShortName = 'P', LongName = "page")] int page = 1,
           [Argument(ShortName = 'S', LongName = "page-size")] int pageSize = 100,
           [Argument(ShortName = 'A', LongName = "auto-page")] bool autoPage = true,
           [Argument(ShortName = 'o', LongName = "output")] string outputFormat = null
        )
        {
            try
            {
                var statementFileRepository = new StatementFileRepository();
                var transactions = statementFileRepository.GetTransactions(this.GetUserPath(this.GetCurrentUser()), id, where, null, sort);
                var summary = transactions.Summary();

                App.Console.Success($" ");
                App.Console.Success($"TOTAL (ENTRADA): " + $"{summary.ValueIn.Format()} ({summary.CountIn})");
                App.Console.Success($"TOTAL (SAÍDA)  : " + $"{summary.ValueOut.Format()} ({summary.CountOut})");
                App.Console.Success($"TOTAL          : " + $"{summary.ValueTotal.Format()} ({summary.CountTotal})");
                App.Console.Success($" ");

                if (page <= 0 || pageSize <= 0)
                {
                    this.ViewFormatted(transactions, outputFormat);
                }
                else
                {
                    this.ViewPagination(page, currentPage =>
                    {
                        var pageResult = transactions.AsQueryable().PageResult(currentPage, pageSize);
                        return pageResult;
                    }, autoPage, outputFormat);
                }

            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }
    }
}
