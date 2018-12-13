using System.Collections.Generic;


namespace Main.Inclusion.Found
{
    public sealed class FoundSqlInclusionEqualityComparer : IEqualityComparer<IFoundSqlInclusion>
    {
        public bool Equals(IFoundSqlInclusion x, IFoundSqlInclusion y)
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
                string.Equals(x.FilePath, y.FilePath) && x.Start == y.Start && x.End == y.End && string.Equals(x.SqlBody, y.SqlBody);
        }

        public int GetHashCode(IFoundSqlInclusion obj)
        {
            unchecked
            {
                int hashCode = (obj.FilePath != null ? obj.FilePath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.Start.Line;
                hashCode = (hashCode * 397) ^ obj.Start.Character;
                hashCode = (hashCode * 397) ^ obj.End.Line;
                hashCode = (hashCode * 397) ^ obj.End.Character;
                hashCode = (hashCode * 397) ^ (obj.SqlBody != null ? obj.SqlBody.GetHashCode() : 0);
                return hashCode;
            }
        }

        private static readonly IEqualityComparer<IFoundSqlInclusion> _instance = new FoundSqlInclusionEqualityComparer();

        public static IEqualityComparer<IFoundSqlInclusion> Instance
        {
            get
            {
                return _instance;
            }
        }

    }
}
