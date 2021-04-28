using System;
using System.Collections.Generic;
using System.Diagnostics;
using Main.Helper;
using Main.Sql.Identifier;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlServerValidator.Visitor;

namespace SqlServerValidator.Identifier
{
    [DebuggerDisplay("{FullTableName}")]
    public class SqlServerTableName : ITableName
    {
        private readonly List<string> _mine;

        public string FullTableName
        {
            get;
        }

        public bool IsRegularTable
        {
            get
            {
                return
                    !IsTempTable;
            }
        }

        public bool IsTempTable
        {
            get
            {
                return
                    FullTableName.IsItTempTable();
            }
        }

        public bool IsTableVariable => false;

        public bool IsCte => false;

        public SqlServerTableName(
            SchemaObjectName objectName
            )
        {
            if (objectName == null)
            {
                throw new ArgumentNullException(nameof(objectName));
            }

            _mine = new List<string>();

            foreach (var identifier in new[] { objectName.BaseIdentifier, objectName.SchemaIdentifier, objectName.DatabaseIdentifier, objectName.ServerIdentifier })
            {
                if (identifier == null)
                {
                    continue;
                }

                var part = identifier.Value;

                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                _mine.Add(part.RemoveParentheses());
            }

            FullTableName = objectName.ToSourceSqlString();
        }

        public bool IsSame(
            string otherTableName
            )
        {
            if (!otherTableName.IsCorrectWildcard())
            {
                throw new ArgumentException("Invalid wild card: " + otherTableName);
            }

            var foreign = otherTableName.PrepareTableName();

            var result = SqlServerTableNameHelper.CompareTableName(
                _mine,
                foreign
                );

            return
                result;
        }


    }
}
