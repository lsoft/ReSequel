using Main.Inclusion.Found;
using System.Collections.Generic;

namespace Main.Inclusion.Validated
{
    public sealed class ValidatedSqlInclusionEqualityComparer : IEqualityComparer<IValidatedSqlInclusion>
    {
        public bool Equals(IValidatedSqlInclusion x, IValidatedSqlInclusion y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (ReferenceEquals(x, null))
            {
                return false;
            }
            if (ReferenceEquals(y, null))
            {
                return false;
            }
            if (x.GetType() != y.GetType())
            {
                return false;
            }
            return
                FoundSqlInclusionEqualityComparer.Instance.Equals(x.Inclusion, y.Inclusion);
        }

        public int GetHashCode(IValidatedSqlInclusion obj)
        {
            return
                FoundSqlInclusionEqualityComparer.Instance.GetHashCode(obj.Inclusion);
        }

        private static readonly IEqualityComparer<IValidatedSqlInclusion> _instance = new ValidatedSqlInclusionEqualityComparer();

        public static IEqualityComparer<IValidatedSqlInclusion> Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
