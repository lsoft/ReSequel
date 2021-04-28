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

        public bool IsRegularTable => false;

        public bool IsTempTable => false;

        public bool IsTableVariable => true;
        public bool IsCte => false;

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
