using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlServerValidator.Visitor
{
    public static class TSqlDomHelpers
    {
        public static string ToSourceSqlString(this TSqlFragment fragment)
        {
            if (fragment.FirstTokenIndex == -1 || fragment.LastTokenIndex == -1)
            {
                return string.Empty;
            }

            if (fragment.FirstTokenIndex == fragment.LastTokenIndex)
            {
                return
                    fragment.ScriptTokenStream[fragment.FirstTokenIndex].Text;
            }

            StringBuilder sqlText = new StringBuilder();
            for (int i = fragment.FirstTokenIndex; i <= fragment.LastTokenIndex; i++)
            {
                sqlText.Append(fragment.ScriptTokenStream[i].Text);
            }

            return
                sqlText.ToString();
        }

        public static string ToSqlString(this TSqlFragment fragment)
        {
            SqlScriptGenerator generator = new Sql140ScriptGenerator();
            string sql;
            generator.GenerateScript(fragment, out sql);
            return sql;
        }

    }

}
