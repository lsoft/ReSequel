using System;
using System.Collections.Generic;
using System.Diagnostics;
using Main.Helper;
using Main.Sql.Identifier;

namespace SqlServerValidator.Identifier
{
    [DebuggerDisplay("{FullTableName}")]
    public class SqlServerTableVariableName : ITableName
    {
        private readonly List<string> _mine;

        public string FullTableName
        {
            get;
        }

        public bool IsTempTable
        {
            get
            {
                return
                    false;
            }
        }

        public bool IsTableVariable
        {
            get
            {
                return
                    true;
            }
        }

        public SqlServerTableVariableName(
            string tableVariableName
            )
        {
            if (tableVariableName == null)
            {
                throw new ArgumentNullException(nameof(tableVariableName));
            }

            FullTableName = tableVariableName;
            _mine = tableVariableName.PrepareTableName();
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
