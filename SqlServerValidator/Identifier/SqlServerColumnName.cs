using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Main.Helper;
using Main.Sql.Identifier;

namespace SqlServerValidator.Identifier
{
    [DebuggerDisplay("{ColumnName}")]
    public class SqlServerColumnName : IColumnName
    {
        private readonly string _mine;

        public string ColumnName
        {
            get;
        }
        public bool IsAlias
        {
            get;
        }

        public bool IsStar
        {
            get
            {
                return ColumnName == "*";
            }
        }

        public SqlServerColumnName(
            string columnName,
            bool isAlias
            )
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            ColumnName = columnName;
            IsAlias = isAlias;
            _mine = columnName.ToString().RemoveParentheses();
        }

        public bool IsSame(string otherColumnName, bool isAlias = false)
        {
            if (string.IsNullOrWhiteSpace(otherColumnName))
            {
                throw new ArgumentException("Incoming column name is empty or null", nameof(otherColumnName));
            }

            if (otherColumnName == "*" && isAlias)
            {
                throw new ArgumentException("Incoming column name is a star AND is a alies at the same time", nameof(otherColumnName));
            }

            if (otherColumnName == "*")
            {
                if (this.ColumnName == "*")
                {
                    return true;
                }

                return
                    false;
            }

            if (this.IsAlias != isAlias)
            {
                return false;
            }

            if (!otherColumnName.IsCorrectWildcard())
            {
                throw new ArgumentException("Invalid wildcard: " + otherColumnName);
            }

            var foreign = otherColumnName.RemoveParentheses();

            var r = Regex.IsMatch(_mine, foreign.WildCardToRegular(), RegexOptions.IgnoreCase);

            return
                r;
        }
    }

}
