using System.Collections.Generic;

namespace NubankCli.Extensions.Tables
{
    public interface ITableConfig<T>
    {
        IEnumerable<TableColumn<T>> GetTableColumns();
    }
}
