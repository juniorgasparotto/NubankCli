using NubankSharp.Extensions.Tables;
using NubankSharp.Entities;
using NubankSharp.Cli.Extensions.Formatters;
using System.Collections.Generic;

namespace NubankSharp.Commands.Transactions
{
    public class TransactionTableConfig : ITableConfig<Transaction>
    {
        public IEnumerable<TableColumn<Transaction>> GetTableColumns()
        {
            yield return new TableColumn<Transaction>
            {
                Name = "Id".ToUpper(),
                ValueFormatter = (t) => t.Id.Format(),
                ValueFormatterWide = (t) => t.Id.ToString(),
            };

            yield return new TableColumn<Transaction>
            {
                Name = "Data postagem".ToUpper(),
                ValueFormatter = (t) => t.PostDate.ToLongDateNoSeconds()
            };

            yield return new TableColumn<Transaction>
            {
                Name = "Data event".ToUpper(),
                ValueFormatter = (t) => t.EventDate.ToLongDateNoSeconds()
            };

            yield return new TableColumn<Transaction>
            {
                Name = "Nome".ToUpper(),
                ValueFormatter = (t) => t.GetNameFormatted()
            };

            yield return new TableColumn<Transaction>
            {
                Name = "Valor".ToUpper(),
                ValueFormatter = (t) => t.Value.Format()
            };

            yield return new TableColumn<Transaction>
            {
                Name = "Cartão".ToUpper(),
                ValueFormatter = (t) => t.CardName,
            };

            yield return new TableColumn<Transaction>
            {
                Name = "Categoria".ToUpper(),
                ValueFormatter = (t) => t.Category,
            };


            yield return new TableColumn<Transaction>
            {
                Name = "Lat".ToUpper(),
                ValueFormatter = (t) => t.Latitude.ToString(),
                OnlyInWide = true
            };

            yield return new TableColumn<Transaction>
            {
                Name = "Lon".ToUpper(),
                ValueFormatter = (t) => t.Longitude.ToString(),
                OnlyInWide = true
            };
        }
    }
}
