using Main.Inclusion.Validated.Result;
using Main.Sql;
using Main.Sql.Identifier;
using Microsoft.CodeAnalysis.Host.Mef;
using SqlServerValidator.Identifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidator.Visitor
{
    public class DatabaseObjectChecker
    {
        private readonly ISqlValidator _sqlValidator;

        public DatabaseObjectChecker(
            ISqlValidator sqlValidator
            )
        {
            if (sqlValidator is null)
            {
                throw new ArgumentNullException(nameof(sqlValidator));
            }


            _sqlValidator = sqlValidator;
        }

        public IModifiedValidationResult CheckForTableOrViewStatus(
            ITableName table,
            bool shouldExists
            )
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            const string CheckForTableOrViewExistsSql = @"
with united_objects (object_id, type)
as
(
	select object_id, type from sys.objects
	union all
	select object_id, type from sys.system_objects
) 
SELECT
	1
FROM united_objects
WHERE 
	object_id = OBJECT_ID(N'{0}') 
	AND type in (N'U', N'V')
";

            if (table.IsTempTable || table.IsTableVariable)
            {
                throw new InvalidOperationException($"Table {table.FullTableName} is temp table or table variable! These types of table cannot be validated against db scheme.");
            }

            var checkResult = _sqlValidator.TryCalculateRowCount(string.Format(CheckForTableOrViewExistsSql, table.FullTableName), out var rowRead);
            var isExists = (checkResult && rowRead > 0);
            if (isExists != shouldExists)
            {
                return ValidationResult.Error(
                    table.FullTableName, 
                    table.FullTableName,
                    isExists
                        ? $"Table {table.FullTableName} already exists!"
                        : $"Table {table.FullTableName} does not exists!"
                    );
            }

            return ValidationResult.Success(table.FullTableName, table.FullTableName);
        }

        public IModifiedValidationResult CheckForIndexStatus(
            IIndexName index,
            bool shouldExists
            )
        {
            if (index == null)
            {
                throw new ArgumentNullException(nameof(index));
            }

            const string CheckForIndexExistsSql = @"
with united_objects (object_id, type)
as
(
	select object_id, type from sys.objects
	union all
	select object_id, type from sys.system_objects
) 
select
	1
from sys.indexes [index]
join united_objects [table] on [index].object_id = [table].object_id
WHERE 
	[table].object_id = OBJECT_ID(N'{0}') 
	AND [table].type in (N'U', N'V')
	AND [index].name = N'{1}'
";

            var checkResult = _sqlValidator.TryCalculateRowCount(string.Format(CheckForIndexExistsSql, index.ParentTable.FullTableName, index.IndexName), out var rowRead);
            var isExists = (checkResult && rowRead > 0);
            if (isExists != shouldExists)
            {
                return ValidationResult.Error(
                    index.CombinedIndexName, 
                    index.CombinedIndexName, 
                    isExists
                        ? $"Index {index.CombinedIndexName} already exists!"
                        : $"Index {index.CombinedIndexName} does not found!"
                    );
            }

            return ValidationResult.Success(index.CombinedIndexName, index.CombinedIndexName);
        }

        internal IModifiedValidationResult CheckForColumnStatus(
            ITableName table,
            IColumnName column,
            bool shouldExists
            )
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            const string CheckForColumnExistsSql = @"
with united_objects (object_id, type)
as
(
	select object_id, type from sys.objects
	union all
	select object_id, type from sys.system_objects
),
united_columns (object_id, name)
as
(
	select object_id, name from sys.all_columns
	union all
	select object_id, name from sys.system_columns
) 
select
	1
from united_columns [columns]
join united_objects [table] on [columns].object_id = [table].object_id
WHERE 
	[table].object_id = OBJECT_ID(N'{0}') 
	AND [table].type in (N'U', N'V')
	AND [columns].name = N'{1}'

";
            var tableAndColumnName = $"{table.FullTableName}.{column.ColumnName}";


            var checkResult = _sqlValidator.TryCalculateRowCount(string.Format(CheckForColumnExistsSql, table.FullTableName, column.ColumnName.RemoveParentheses()), out var rowRead);
            var isExists = (checkResult && rowRead > 0);
            if (isExists != shouldExists)
            {
                return ValidationResult.Error(
                    tableAndColumnName,
                    tableAndColumnName,
                    isExists
                        ? $"Column {tableAndColumnName} already exists!"
                        : $"Column {tableAndColumnName} does not found!"
                    );
            }

            return ValidationResult.Success(tableAndColumnName, tableAndColumnName);
        }
    }
}
