using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Main.Helper
{
    public static partial class TypeSymbolHelper
    {
        public static bool TryGetStringValue(
            this Compilation compilation,
            Document document,
            ExpressionSyntax expression,
            out string result
        )
        {
            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var literal = expression as LiteralExpressionSyntax;
                result = literal.Token.ValueText;
                return true;
            }

            if (expression.IsKind(SyntaxKind.AddExpression))
            {
                var binaryExpression = expression as BinaryExpressionSyntax;
                if (TryGetStringValue(compilation, document, binaryExpression.Left, out var left))
                {
                    if (TryGetStringValue(compilation, document, binaryExpression.Right, out var right))
                    {
                        result = left + right;
                        return true;
                    }
                }

                result = null;
                return false;
            }

            var model = compilation.GetSemanticModel(expression.SyntaxTree);
            if (model == null)
            {
                result = null;
                return false;
            }

            var symbol = ModelExtensions.GetSymbolInfo(model, expression).Symbol;

            if (symbol == null)
            {
                result = null;
                return false;
            }

            var references = symbol.DeclaringSyntaxReferences.ToList();

            if (references.Count != 1)
            {
                result = null;
                return false;
            }

            //do searching for additional references, like additional assignments
            //in that case we skip this symbol usage due to uncertanity about REAL SQL body
            var globalReferences = SymbolFinder.FindReferencesAsync(
                symbol,
                document.Project.Solution,
                new List<Document> { document }.ToImmutableHashSet()
                ).Result.ToList();

            if (globalReferences.Count != 1)
            {
                result = null;
                return false;
            }

            var globalLocations = globalReferences[0].Locations.ToList();

            if (globalLocations.Count != 1)
            {
                result = null;
                return false;
            }

            var defNode = references[0].GetSyntax();

            var valueClause = defNode.DescendantNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();
            if (valueClause != null)
            {
                return TryGetStringValue(compilation, document, valueClause.Value, out result);
            }

            result = null;
            return false;
        }


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
