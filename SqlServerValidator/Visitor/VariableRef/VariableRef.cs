using System;
using Main.Sql.VariableRef;

namespace SqlServerValidator.Visitor.VariableRef
{
    public class VariableRef : IVariableRef2
    {
        public string Name
        {
            get;
        }

        public int ReferenceCount
        {
            get;
            private set;
        }

        public bool IsSame(string otherVariableName)
        {
            return 
                SqlVariableStringComparer.Instance.Equals(this.Name, otherVariableName);
        }

        public VariableRef(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(name);
            }

            Name = name;
            ReferenceCount = 0;
        }

        public void IncrementReferenceCount()
        {
            ReferenceCount++;
        }
    }
}