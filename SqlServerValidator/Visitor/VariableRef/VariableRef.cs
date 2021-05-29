using System;
using Main.Sql.VariableRef;

namespace SqlServerValidator.Visitor.VariableRef
{
    public class VariableRef : IVariableRef2
    {
        private readonly bool _forceToSetUnknownProcessingScope;

        public string Name
        {
            get;
        }

        public int ReferenceCount
        {
            get;
            private set;
        }

        public bool IsInScopeOfUnknownProcessing => ReferenceCount > 1 || _forceToSetUnknownProcessingScope;

        public bool IsSame(string otherVariableName)
        {
            return 
                SqlVariableStringComparer.Instance.Equals(this.Name, otherVariableName);
        }

        public VariableRef(string name, bool forceToSetUnknownProcessingScope = false)
        {
            if (name == null)
            {
                throw new ArgumentNullException(name);
            }

            Name = name;
            _forceToSetUnknownProcessingScope = forceToSetUnknownProcessingScope;
            ReferenceCount = 0;
        }

        public void IncrementReferenceCount()
        {
            ReferenceCount++;
        }

    }
}