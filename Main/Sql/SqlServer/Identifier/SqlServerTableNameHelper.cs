
using Main.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Main.Sql.SqlServer.Identifier
{
    public static class SqlServerTableNameHelper
    {

        public static bool CompareTableName(
            List<string> mine,
            List<string> foreign
            )
        {
            for (var cc = 0; cc < Math.Min(mine.Count, foreign.Count); cc++)
            {
                //var result = StringComparer.InvariantCultureIgnoreCase.Compare(
                //    mine[cc],
                //    foreign[cc]
                //    );

                //if (result != 0)
                //{
                //    return false;
                //}

                var r = Regex.IsMatch(mine[cc], foreign[cc].WildCardToRegular(), RegexOptions.IgnoreCase);

                if(!r)
                {
                    return false;
                }
            }

            return
                true;
        }

        public static List<string> PrepareTableName(
            this string identifier
            )
        {
            var parts = identifier
                .Split('.')
                ;

            var result = parts
                .Reverse()
                .Select(k => k.RemoveParentheses())
                .ToList()
                ;

            return result;
        }

        public static string RemoveParentheses(
            this string lexem
            )
        {
            if (lexem.Length > 2)
            {
                if (lexem.StartsWith("[") && lexem.EndsWith("]"))
                {
                    lexem = lexem.Substring(1, lexem.Length - 2);
                }
            }

            //if (lexem.StartsWith("["))
            //{
            //    lexem = lexem.Substring(1);
            //}

            //if (lexem.EndsWith("]"))
            //{
            //    lexem = lexem.Substring(0, lexem.Length - 1);
            //}

            return lexem;
        }
    }
}
