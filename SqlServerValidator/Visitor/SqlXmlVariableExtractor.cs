using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlServerValidator.Visitor
{
    public class SqlXmlVariableExtractor
    {
        public const string SqlXmlVariableRegexBody = @"sql\s?:\s?variable\s?\(\s?""\s?@([^""\s]+)\s?""\s?\)";

        private readonly Regex _regex = new Regex(SqlXmlVariableRegexBody);


        public IEnumerable<string> ExtractNames(string sql)
        {
            if (sql is null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            var matches = _regex.Matches(sql);
            foreach (Match match in matches)
            {
                yield return "@" + match.Groups[1];
            }
        }
    }

}
