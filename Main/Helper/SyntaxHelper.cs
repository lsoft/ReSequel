using Microsoft.CodeAnalysis;

namespace Main.Helper
{
    public static class SyntaxHelper
    {
        public static string GetFullTypeName(
            this ISymbol symbol
            )
        {
            if (symbol == null)
            {
                return string.Empty;
            }

            var localSymbol = symbol as ILocalSymbol;
            if (localSymbol == null)
            {
                return string.Empty;
            }

            return
                localSymbol.Type.ToString();

        }

        public static string GetFullTypeName(
            this SemanticModel model,
            SyntaxNode node
            )
        {
            if (node == null)
            {
                return string.Empty;
            }

            ITypeSymbol subjectType = model.GetTypeInfo(node).Type;

            if (subjectType == null)
            {
                return string.Empty;
            }

            if (subjectType.ContainingNamespace == null)
            {
                return subjectType.Name;
            }

            return
                subjectType.ContainingNamespace + "." + subjectType.Name;
        }

        public static string GetFullTypeNameOfContainingClass(
            this ISymbol symbol
            )
        {
            if (symbol == null)
            {
                return string.Empty;
            }

            var containingSymbol = symbol.ContainingSymbol;

            if(containingSymbol == null)
            {
                return string.Empty;
            }

            if (containingSymbol != null && containingSymbol.ContainingNamespace != null)
            {
                return
                    containingSymbol.ContainingNamespace.ToString() + "." + containingSymbol.Name;
            }

            return
                containingSymbol.Name;
        }


        public static (int start, int end) GetStartEnd(
            this SyntaxNode node
            )
        {
            int sstart;
            int eend;

            var leadingTrivia = node.GetLeadingTrivia();
            if (leadingTrivia.Count > 0)
            {
                sstart = leadingTrivia.FullSpan.End;//+ 1;
            }
            else
            {
                sstart = node.FullSpan.Start;
            }

            var trailingTrivia = node.GetTrailingTrivia();
            if (trailingTrivia.Count > 0)
            {
                eend = trailingTrivia.FullSpan.Start;//- 1;
            }
            else
            {
                eend = node.FullSpan.End;
            }

            return
                (sstart, eend);
        }
    }
}
