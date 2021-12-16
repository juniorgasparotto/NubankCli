using System.Collections.Generic;

namespace NubankSharp.Extensions.Tables
{
    public interface ITableConfig<T>
    {
        IEnumerable<TableColumn<T>> GetTableColumns();
    }
}
