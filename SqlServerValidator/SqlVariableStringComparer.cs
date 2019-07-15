using System;
using System.Collections.Generic;

namespace SqlServerValidator
{
    public class SqlVariableStringComparer : IEqualityComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null || y == null)
            {
                return
                    StringComparer.InvariantCultureIgnoreCase.Compare(x, y);
            }

            var xnorm = SqlParameterNormalize(x);
            var ynorm = SqlParameterNormalize(y);

            return
                StringComparer.InvariantCultureIgnoreCase.Compare(xnorm, ynorm);
        }

        public bool Equals(string x, string y)
        {
            if (x == null || y == null)
            {
                return
                    StringComparer.InvariantCultureIgnoreCase.Equals(x, y);
            }

            var xnorm = SqlParameterNormalize(x);
            var ynorm = SqlParameterNormalize(y);

            return
                StringComparer.InvariantCultureIgnoreCase.Equals(xnorm, ynorm);
        }

        public int GetHashCode(string obj)
        {
            return
                obj.GetHashCode();
        }

        public static string SqlParameterNormalize(string x)
        {
            var xnorm = x;

            while (x.StartsWith("@")) //muhahahaha!!! I know that while is inefficient in case of a lot of @, but it's hard to imagine this scenario in a real world
            {
                x = x.Substring(1);
            }

            return x;
        }

        public static SqlVariableStringComparer Instance = new SqlVariableStringComparer();

    }
}
