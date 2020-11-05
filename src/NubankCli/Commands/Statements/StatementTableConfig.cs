using NubankCli.Extensions.Tables;
using NubankCli.Core.Entities;
using NubankCli.Core.Extensions;
using NubankCli.Core.Extensions.Formatters;
using System.Collections.Generic;
using System.Linq;

namespace NubankCli.Commands.Statements
{
    public class StatementTableConfig : ITableConfig<Statement>
    {
        public IEnumerable<TableColumn<Statement>> GetTableColumns()
        {
            yield return new TableColumn<Statement>
            {
                Name = "Cartão".ToUpper(),
                ValueFormatter = (s) => (s.Card?.Name) ?? "-"
            };

            yield return new TableColumn<Statement>
            {
                Name = "Início".ToUpper(),
                ValueFormatter = (s) => s.Start.ToString("yyyy/MM/dd"),
            };

            yield return new TableColumn<Statement>
            {
                Name = "Fim".ToUpper(),
                ValueFormatter = (s) => s.End.ToString("yyyy/MM/dd"),
            };

            yield return new TableColumn<Statement>
            {
                Name = "Entrada".ToUpper(),
                ValueFormatter = (s) =>
                {
                    var tIn = s.Transactions.GetIn();
                    return $"{tIn.Total().Format()} ({tIn.Count()})";
                }
            };

            yield return new TableColumn<Statement>
            {
                Name = "Saída".ToUpper(),
                ValueFormatter = (s) =>
                {
                    var tOut = s.Transactions.GetOut();
                    return $"{tOut.Total().Format()} ({tOut.Count()})";
                }
            };

            yield return new TableColumn<Statement>
            {
                Name = "Total".ToUpper(),
                ValueFormatter = (s) => $"{s.Transactions.Total().Format()} ({s.Transactions.Count()})",
            };

            yield return new TableColumn<Statement>
            {
                Name = "Tipo de extrato".ToUpper(),
                ValueFormatter = (s) => s.StatementType.ToString(),
                OnlyInWide = true
            };
        }
    }
}
