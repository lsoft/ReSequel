using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Main.Helper;
using Main.Sql.Identifier;

namespace SqlServerValidator.Identifier
{
    [DebuggerDisplay("{FullTableName}")]
    public class SqlServerCteName : ITableName
    {
        public string FullTableName
        {
            get;
        }

        public bool IsRegularTable => false;

        public bool IsTempTable => false;

        public bool IsTableVariable => false;

        public bool IsCte => true;

        public SqlServerCteName(
            string fullTableName
            )
        {
            if (fullTableName == null)
            {
                throw new ArgumentNullException(nameof(fullTableName));
            }

            FullTableName = fullTableName;
        }

        public bool IsSame(
            string otherTableName
            )
        {
            if (!otherTableName.IsCorrectWildcard())
            {
                throw new ArgumentException("Invalid wild card: " + otherTableName);
            }

            var result = Regex.IsMatch(FullTableName, otherTableName.WildCardToRegular(), RegexOptions.IgnoreCase);

            return
                result;
        }
    }
}
