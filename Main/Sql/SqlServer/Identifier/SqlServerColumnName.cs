using Main.Helper;
using Main.Sql.Identifier;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Main.Sql.SqlServer.Identifier
{
    [DebuggerDisplay("{ColumnName}")]
    public class SqlServerColumnName : IColumnName
    {
        private readonly string _mine;

        public string ColumnName
        {
            get;
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
                throw new ArgumentException("message", nameof(otherColumnName));
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
                throw new ArgumentException("Invalid wild card: " + otherColumnName);
            }

            var foreign = otherColumnName.RemoveParentheses();

            //return
            //    StringComparer.InvariantCultureIgnoreCase.Compare(_mine, foreign) == 0;
            var r = Regex.IsMatch(_mine, foreign.WildCardToRegular(), RegexOptions.IgnoreCase);

            return
                r;
        }
    }

}
