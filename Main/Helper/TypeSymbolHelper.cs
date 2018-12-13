using Microsoft.CodeAnalysis;
using System;

namespace Main.Helper
{
    public static partial class TypeSymbolHelper
    {
        public static bool CanBeCastedTo(
            this ITypeSymbol target,
            string subjectTypeFullName
            )
        {
            var roslynTypeFullName = target.ContainingNamespace.Name + "." + target.Name;

            if (StringComparer.InvariantCultureIgnoreCase.Compare(roslynTypeFullName, subjectTypeFullName) == 0)
            {
                return true;
            }

            foreach (INamedTypeSymbol @interface in target.AllInterfaces)
            {
                var roslynInterfaceFullName = @interface.ContainingNamespace.Name + "." + @interface.Name;

                if (StringComparer.InvariantCultureIgnoreCase.Compare(roslynInterfaceFullName, subjectTypeFullName) == 0)
                {
                    return true;
                }
            }

            if (target.BaseType != null && target.BaseType != target)
            {
                if (CanBeCastedTo(target.BaseType, subjectTypeFullName))
                {
                    return true;
                }
            }

            foreach (INamedTypeSymbol @interface in target.AllInterfaces)
            {
                var r = CanBeCastedTo(
                    @interface,
                    subjectTypeFullName
                    );

                if (r)
                {
                    return true;
                }
            }

            return false;
        }
    }

}
