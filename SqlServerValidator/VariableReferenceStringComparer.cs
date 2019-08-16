using System.Collections.Generic;
using Main.Sql.VariableRef;

namespace SqlServerValidator
{
    public class VariableReferenceStringComparer : IEqualityComparer<IVariableRef>
    {
        public int Compare(IVariableRef x, IVariableRef y)
        {
            return
                SqlVariableStringComparer.Instance.Compare(x?.Name, y?.Name);
        }

        public bool Equals(IVariableRef x, IVariableRef y)
        {
            return
                SqlVariableStringComparer.Instance.Equals(x?.Name, y?.Name);
        }

        public int GetHashCode(IVariableRef obj)
        {
            return
                obj.GetHashCode();
        }

        public static VariableReferenceStringComparer Instance = new VariableReferenceStringComparer();

    }
}