using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using NubankCli.Core.Extensions.Formatters;

namespace NubankCli.Extensions.Tables
{
    /// <summary>
    /// This class brings the output tabled feature that can be very useful to present]
    /// information quickly and more visually organized. Of course everything depends on 
    /// the amount of information you want to display, the higher, the worse the view.
    /// </summary>
    public class TableView
    {
        /// <summary>
        /// Columns definitions
        /// </summary>
        public List<ColumnDefinition> ColumnsDefinition { get; private set; }

        /// <summary>
        /// List of rows
        /// </summary>
        public List<IRow> Rows { get; private set; }

        /// <summary>
        /// Simulate padding left
        /// </summary>
        public int PaddingLeft { get; set; }

        /// <summary>
        /// Simulate padding top
        /// </summary>
        public int PaddingTop { get; set; }

        /// <summary>
        /// Simulate a padding bottom
        /// </summary>
        public int PaddingBottom { get; set; }

        /// <summary>
        /// Determines whether include header
        /// </summary>
        public bool IncludeHeader { get; set; }

        /// <summary>
        /// Determines the char that represents the line separator
        /// </summary>
        public char LineSeparator { get; set; }

        /// <summary>
        /// Determines the string that represents the column separator
        /// </summary>
        public string ColumnSeparator { get; set; }

        /// <summary>
        /// StringBuilder reference
        /// </summary>
        public StringBuilder StrBuilder { get; set; }

        /// <summary>
        /// Determines whether add line separator in output
        /// </summary>
        public bool AddLineSeparator { get; set; }

        /// <summary>
        /// Determines whether add column separator in output
        /// </summary>
        public bool AddColumnSeparator { get; set; }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="strBuilder">StringBuilder reference</param>
        public TableView(StringBuilder strBuilder = null)
        {
            this.ColumnsDefinition = new List<ColumnDefinition>();
            this.Rows = new List<IRow>();
            this.StrBuilder = strBuilder ?? new StringBuilder();
            this.AddLineSeparator = true;
            this.AddColumnSeparator = true;
            this.LineSeparator = '-';
            this.ColumnSeparator = "| ";
            this.IncludeHeader = false;
        }

        /// <summary>
        /// Add column definition
        /// </summary>
        /// <param name="name">Name of column</param>
        /// <param name="width">Column width</param>
        /// <param name="paddingLeft">Simulate padding left</param>
        /// <param name="paddingRight">Simulate padding right</param>
        /// <returns>Return the column definition</returns>
        public ColumnDefinition AddColumnDefinition(string name, int width = 0, int paddingLeft = 0, int paddingRight = 0)
        {
            var column = new ColumnDefinition()
            {
                Name = name,
                PaddingLeft = paddingLeft,
                PaddingRight = paddingRight,
                Width = width
            };
            this.ColumnsDefinition.Add(column);
            return column;
        }

        /// <summary>
        /// Add a row summary. It's a row without columns
        /// </summary>
        /// <param name="text">Row text</param>
        /// <param name="width">Row width</param>
        /// <param name="paddingLeft">Simulate a padding left</param>
        /// <returns>Return a row summary</returns>
        public RowSummary AddRowSummary(string text, int width = 0, int paddingLeft = 0)
        {
            var row = new RowSummary()
            {
                Text = text,
                Width = width,
                PaddingLeft = paddingLeft
            };
            this.Rows.Add(row);
            return row;
        }

        /// <summary>
        /// Add a new row
        /// </summary>
        /// <returns>Return a row</returns>
        public Row AddRow()
        {
            var row = new Row();
            this.Rows.Add(row);
            return row;
        }

        private void InsertColumnSeparator(List<IRow> rows)
        {
            if (this.AddColumnSeparator)
            {
                foreach (var row in rows)
                {
                    if (row is Row)
                    {
                        var rowNormal = (Row)row;
                        var first = rowNormal.Columns.FirstOrDefault();
                        var last = rowNormal.Columns.LastOrDefault();
                        foreach (var col in rowNormal.Columns)
                            if (col != first)
                                col.Text = this.ColumnSeparator + col.Text;
                    }
                }
            }
        }

        private void InsertLineSeparator(List<IRow> rows)
        {
            if (AddLineSeparator)
            {
                var maxLength = 0;
                foreach (var row in rows)
                {
                    if (row is Row)
                    {
                        var rowNormal = (Row)row;
                        var maxLengthLine = rowNormal.Columns.Sum(f => f.Text.Length);
                        if (maxLengthLine > maxLength)
                            maxLength = maxLengthLine;
                    }
                    else if (row is RowSummary)
                    {
                        var rowSummary = (RowSummary)row;
                        if (rowSummary.Text.Length > maxLength)
                            maxLength = rowSummary.Text.Length;
                    }

                }

                foreach (var row in rows.ToList())
                {
                    var newRow = new RowLine();
                    newRow.Text = new string(this.LineSeparator, maxLength);
                    rows.Insert(rows.IndexOf(row) + 1, newRow);
                }
            }
        }

        private void ChunkRows(List<IRow> rows)
        {
            var rowsSummaries = Rows.Where(f => f.GetType() == typeof(RowSummary)).Cast<RowSummary>().ToList();
            foreach (var summary in rowsSummaries)
            {
                var newsLines = this.ChunksWords(summary.Text, summary.Width);
                if (newsLines.Count() > 1)
                {
                    var newRowsForThisRow = new List<RowSummary>();
                    foreach (var newLine in newsLines)
                    {
                        var rowAdd = new RowSummary();
                        rowAdd.Width = summary.Width;
                        rowAdd.PaddingLeft = summary.PaddingLeft;
                        rowAdd.Text = newLine;
                        newRowsForThisRow.Add(rowAdd);
                    }

                    var index = rows.IndexOf(summary);
                    rows.Remove(summary);
                    rows.InsertRange(index, newRowsForThisRow);
                }
            }

            var rowsNormal = rows.Where(f => f.GetType() == typeof(Row)).Cast<Row>().ToList();
            foreach (var row in rowsNormal)
            {
                var newRowsForThisRow = new List<Row>();
                var indexColumn = 0;
                foreach (var column in row.Columns)
                {
                    var definition = this.ColumnsDefinition[indexColumn];
                    //column.Text = column.Text != null ? column.Text.TrimEnd('\r', '\n') : null;

                    var newsLines = this.ChunksWords(column.Text, definition.Width).ToArray();

                    if (newsLines.Length > 1)
                    {
                        var indexNewRow = 0;
                        foreach (var newLine in newsLines)
                        {
                            Row rowNew = newRowsForThisRow.ElementAtOrDefault(indexNewRow);
                            if (rowNew == null)
                            {
                                rowNew = new Row();
                                rowNew.IsMainRow = indexNewRow == 0 ? true : false;
                                foreach (var columnCopy in row.Columns)
                                {
                                    if (rowNew.IsMainRow && columnCopy != column)
                                        rowNew.AddColumnInRow(columnCopy.Text);
                                    else if (columnCopy == column)
                                        rowNew.AddColumnInRow(newLine);
                                    else
                                        rowNew.AddColumnInRow("");
                                }

                                newRowsForThisRow.Add(rowNew);
                            }
                            else
                            {
                                rowNew.Columns[indexColumn].Text = newLine;
                            }

                            indexNewRow++;
                        }
                    }

                    indexColumn++;
                }

                if (newRowsForThisRow.Any())
                {
                    var index = rows.IndexOf(row);
                    rows.Remove(row);
                    rows.InsertRange(index, newRowsForThisRow);
                }
            }
        }

        internal void AddRowSummary(object helpFooterDesc)
        {
            throw new NotImplementedException();
        }

        private void AddPaddingInRows(List<IRow> rows)
        {
            var maxPaddingsColumns = this.GetMaxPaddingsForEachColumn(rows);

            foreach (var row in rows)
            {
                if (row is RowSummary)
                {
                    var summary = ((RowSummary)row);
                    var text = summary.Text;
                    if (summary.PaddingLeft > 0)
                        summary.Text = text.PadLeft(text.Length + summary.PaddingLeft);
                }
                else if (row is Row)
                {
                    this.AddPaddingInRow((Row)row, maxPaddingsColumns);
                }
            }
        }

        private void AddPaddingInRow(Row row, int[] maxColumnsWidthAuto)
        {
            for (int i = 0; i < row.Columns.Count; i++)
            {
                var column = row.Columns[i];
                var value = column.Text;

                // Append the value with padding of the maximum length of any value for this element
                if (i != row.Columns.Count - 1)
                    value = (column.Text ?? "").PadRight(maxColumnsWidthAuto[i]);

                if (this.ColumnsDefinition[i].PaddingLeft > 0)
                    value = value.PadLeft(value.Length + this.ColumnsDefinition[i].PaddingLeft, ' ');

                column.Text = value;
            }
        }

        /// <summary>
        /// Build all information and create the TableView. Use ToString() to get the result
        /// </summary>
        /// <returns></returns>
        public TableView Build()
        {
            this.Validate();
            var newRows = this.Rows.ToList();

            this.AddRowsHeader(newRows);
            this.ChunkRows(newRows);
            this.AddPaddingInRows(newRows);
            this.InsertColumnSeparator(newRows);
            this.InsertLineSeparator(newRows);

            this.AddPaddingTop();
            var lastRow = newRows.LastOrDefault();
            foreach (var row in newRows)
            {
                this.StrBuilder.Append(this.GetLine(row));
                if (row != lastRow)
                    StrBuilder.AppendLine();
            }
            this.AddPaddingBottom();
            return this;
        }

        private void AddRowsHeader(List<IRow> newRows)
        {
            if (this.IncludeHeader)
            {
                var rowHeader = new Row();
                foreach (var colDefinition in this.ColumnsDefinition)
                    rowHeader.AddColumnInRow(colDefinition.Name);
                newRows.Insert(0, rowHeader);
            }
        }

        private void AddPaddingTop()
        {
            if (this.PaddingTop > 0)
                for (var i = 1; i <= this.PaddingTop; i++)
                    this.StrBuilder.AppendLine();
        }

        private void AddPaddingBottom()
        {
            if (this.PaddingBottom > 0)
                for (var i = 1; i <= this.PaddingBottom; i++)
                    this.StrBuilder.AppendLine();
        }

        private string GetLine(IRow row)
        {
            var line = row.ToString();
            if (this.PaddingLeft == 0)
                return line;
            else
                return line.PadLeft(line.Length + this.PaddingLeft);
        }

        private int[] GetMaxPaddingsForEachColumn(List<IRow> rows)
        {
            var rowsNormal = rows.Where(f => f.GetType() == typeof(Row)).Cast<Row>();
            var numElements = this.ColumnsDefinition.Count;
            var maxValues = new int[numElements];
            for (int i = 0; i < numElements; i++)
            {
                maxValues[i] = rowsNormal.Max(row => (row.Columns[i].Text ?? "").Length) + this.ColumnsDefinition[i].PaddingRight;
            }
            return maxValues;
        }

        private IEnumerable<string> ChunksWords(string str, int chunkSize)
        {
            var lst = new List<string>();
            if (chunkSize > 0)
            {
                str = str ?? "";
                var lines = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    int partLength = chunkSize;
                    string sentence = line;
                    //string[] words = sentence.Split(new string[] { " " } , StringSplitOptions.None);
                    string[] words = Regex.Split(str, @"(\s+)");
                    var parts = new Dictionary<int, string>();
                    string part = string.Empty;
                    int partCounter = 0;
                    foreach (var word in words)
                    {
                        if (part.Length + word.Length <= partLength)
                        {
                            part += word;
                        }
                        else
                        {
                            parts.Add(partCounter, part);
                            part = word;
                            partCounter++;
                        }
                    }
                    parts.Add(partCounter, part);
                    foreach (var item in parts)
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                            lst.Add(item.Value);
                    }
                }

                for (var i = 1; i < lst.Count; i++)
                    lst[i] = lst[i].TrimStart(' ');

                lst.RemoveAll(f => string.IsNullOrEmpty(f));
            }

            return lst;
        }

        private void Validate()
        {
            var rowsNormal = this.Rows.Where(f => f.GetType() == typeof(Row)).Cast<Row>();
            var indexLine = 0;
            foreach (var row in rowsNormal)
            {
                if (row.Columns.Count < this.ColumnsDefinition.Count)
                    throw new Exception(string.Format("Line {0} have less columns than the number of column definition (total expected: {1}): {2}", indexLine, this.ColumnsDefinition.Count, row.ToString()));

                if (row.Columns.Count > this.ColumnsDefinition.Count)
                    throw new Exception(string.Format("Line {0} have more columns than the number of column definition (total expected: {1}): {2}", indexLine, this.ColumnsDefinition.Count, row.ToString()));

                indexLine++;
            }
        }

        public override string ToString()
        {
            return this.StrBuilder.ToString();
        }

        /// <summary>
        /// Helper to create a table view for a specificy type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="colWidth"></param>
        /// <returns></returns>
        public static TableView ToTableView<T>(IEnumerable<T> list, ITableConfig<T> tableConfig, bool wide, int colWidth = 0)
        {
            var table = new TableView();
            table.IncludeHeader = true;
            var columns = wide ? tableConfig.GetTableColumns() : tableConfig.GetTableColumns().Where(p => p.OnlyInWide == false);

            foreach (var col in columns)
            {
                table.AddColumnDefinition(col.Name ?? "[?]", colWidth, 0, 3);
            }

            foreach (var obj in list)
            {
                var row = table.AddRow();
                foreach (var column in columns)
                {
                    var value = wide && column.ValueFormatterWide != null ? column.ValueFormatterWide(obj) : column.ValueFormatter(obj);
                    value = value?.Trim() ?? "";
                    row.AddColumnInRow(value);
                }
            }

            return table;
        }

        /// <summary>
        /// Helper to create a table view for a specificy type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="colWidth"></param>
        /// <returns></returns>
        public static TableView ToTableView<T>(IEnumerable<T> list, int colWidth = 0, bool wide = false)
        {
            // OBS:
            // Os formats serão executados duas vezes pois a primeira pode cair num formatter que 
            // retorna um valor muito grande (ex: formats de IEnumerable podem resultar em string grandes)
            // Com isso garante que o segundo format não vai deixar a string vazar

            var table = new TableView();
            table.IncludeHeader = true;

            if (list.Any() && list.ElementAt(0) is JObject first)
            {
                foreach (var prop in first)
                {
                    table.AddColumnDefinition(prop.Key.ToUpper(), colWidth, 0, 3);
                }

                foreach (var obj in list)
                {
                    var row = table.AddRow();
                    var jObject = obj as JObject;
                    foreach (var prop in jObject)
                    {
                        string value;
                        if (wide)
                            value = prop.Value?.ToString();
                        else
                            value = prop.Value.Format().Format();

                        row.AddColumnInRow(value ?? "");
                    }
                }
            }
            else if (list.Any() && list.ElementAt(0) is ExpandoObject first2)
            {
                foreach (var prop in first2)
                {
                    table.AddColumnDefinition(prop.Key.ToUpper(), colWidth, 0, 3);
                }

                foreach (var obj in list)
                {
                    var row = table.AddRow();
                    var expando = obj as ExpandoObject;
                    foreach (var prop in expando)
                    {
                        string value;
                        if (wide)
                        {
                            if (prop.Value is ExpandoObject eo)
                                value = eo.Format();
                            else
                                value = prop.Value?.ToString();
                        }
                        else
                        {
                            value = prop.Value.Format().Format();
                        }

                        row.AddColumnInRow(value ?? "");
                    }
                }
            }
            else
            {
                var properties = typeof(T).GetProperties();
                foreach (var prop in properties)
                {
                    table.AddColumnDefinition(prop.Name.ToUpper(), colWidth, 0, 3);
                }

                foreach (var obj in list)
                {
                    var row = table.AddRow();
                    foreach (var prop in properties)
                    {
                        string value;
                        if (wide)
                            value = prop.GetValue(obj)?.ToString();
                        else
                            value = prop.GetValue(obj).Format().Format();

                        row.AddColumnInRow(value ?? "");
                    }
                }
            }

            return table;
        }

        #region Classes

        /// <summary>
        /// Represent the base row
        /// </summary>
        public interface IRow
        {
            int CountChars { get; set; }
        }

        /// <summary>
        /// Represent the row with columns
        /// </summary>
        public class Row : IRow
        {
            /// <summary>
            /// List of columns
            /// </summary>
            public List<Column> Columns { get; private set; }
            internal bool IsMainRow { get; set; }
            public int CountChars { get; set; }

            /// <summary>
            /// Initialize
            /// </summary>
            public Row()
            {
                this.Columns = new List<Column>();
                this.IsMainRow = true;
            }

            /// <summary>
            /// Add a column in row
            /// </summary>
            /// <param name="text">Column text</param>
            /// <returns>The same row</returns>
            public Row AddColumnInRow(string text)
            {
                var column = new Column() { Text = text };
                this.CountChars += text?.Length ?? 0;
                this.Columns.Add(column);
                return this;
            }

            public override string ToString()
            {
                return string.Join("", this.Columns.Select(f => f.Text));
            }
        }

        /// <summary>
        /// Represent the single row line
        /// </summary>
        public class RowLine : IRow
        {
            /// <summary>
            /// Row line text
            /// </summary>
            public string Text { get; set; }
            public int CountChars { get; set; }

            public override string ToString()
            {
                return this.Text;
            }
        }

        /// <summary>
        /// Represent the row summary. Row without columns.
        /// </summary>
        public class RowSummary : IRow
        {
            /// <summary>
            /// Row text
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Simulate the padding left
            /// </summary>
            public int PaddingLeft { get; set; }

            /// <summary>
            /// Row width
            /// </summary>
            public int Width { get; set; }

            public int CountChars { get; set; }

            public override string ToString()
            {
                return this.Text;
            }
        }

        /// <summary>
        /// Represent columns of rows
        /// </summary>
        public class Column
        {
            /// <summary>
            /// Column text
            /// </summary>
            public string Text { get; set; }

            public override string ToString()
            {
                return this.Text;
            }
        }

        /// <summary>
        /// Represent the colum definition
        /// </summary>
        public class ColumnDefinition
        {
            /// <summary>
            /// Column name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Simulate padding left
            /// </summary>
            public int PaddingLeft { get; set; }

            /// <summary>
            /// Simulate padding right
            /// </summary>
            public int PaddingRight { get; set; }

            /// <summary>
            /// Column width
            /// </summary>
            public int Width { get; set; }
        }

        #endregion
    }

}