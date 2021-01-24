using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Main.Sql.Identifier;
using Microsoft.SqlServer.Dac.Model;

namespace SqlServerValidator.Identifier
{
    [DebuggerDisplay("{ParentTable}.{IndexName}")]
    public class SqlServerIndexName : IIndexName
    {
        private readonly string _mine;

        public ITableName ParentTable
        {
            get;
        }

        public string IndexName
        {
            get;
        }

        public string CombinedIndexName => $"{ParentTable.FullTableName}.{IndexName}";

        public SqlServerIndexName(
            ITableName parentTable,
            string indexName
            )
        {
            if (parentTable is null)
            {
                throw new ArgumentNullException(nameof(parentTable));
            }

            if (indexName is null)
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            ParentTable = parentTable;
            IndexName = indexName;

            _mine = indexName.ToString().RemoveParentheses();
        }

        public bool IsSame(string tableName, string indexName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Incoming table name is empty or null", nameof(tableName));
            }
            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentException("Incoming index name is empty or null", nameof(indexName));
            }

            if (!ParentTable.IsSame(tableName))
            {
                return false;
            }

            var foreign = indexName.RemoveParentheses();

            var r = Regex.IsMatch(_mine, foreign, RegexOptions.IgnoreCase);

            return
                r;
        }
    }
}
