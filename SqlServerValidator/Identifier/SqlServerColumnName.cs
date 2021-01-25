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

        public bool IsStar
        {
            get
            {
                return ColumnName == "*";
            }
        }

        public SqlServerColumnName(
            string columnName
            )
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            ColumnName = columnName;
            _mine = columnName.ToString().RemoveParentheses();
        }

        public bool IsSame(string otherColumnName)
        {
            if (string.IsNullOrWhiteSpace(otherColumnName))
            {
                throw new ArgumentException("Incoming column name is empty or null", nameof(otherColumnName));
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
