using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Main.Helper;
using Main.Sql.Identifier;

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
            if (tableName == null)
            {
                throw new ArgumentException("Incoming table name is null", nameof(tableName));
            }
            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentException("Incoming index name is empty or null", nameof(indexName));
            }

            if (!indexName.IsCorrectWildcard())
            {
                throw new ArgumentException("Invalid index name wild card: " + indexName);
            }

            if (!string.IsNullOrEmpty(tableName))
            {
                if (!tableName.IsCorrectWildcard())
                {
                    throw new ArgumentException("Invalid table name wild card: " + tableName);
                }

                if (!ParentTable.IsSame(tableName))
                {
                    return false;
                }
            }

            var foreign = indexName.RemoveParentheses();

            var r = Regex.IsMatch(_mine, foreign.WildCardToRegular(), RegexOptions.IgnoreCase);

            return
                r;
        }
    }
}
