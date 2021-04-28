using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Task = System.Threading.Tasks.Task;
using System.Threading.Tasks;

namespace Main.Helper
{
    public static partial class TypeSymbolHelper
    {
        public static async Task<string> TryGetStringValueAsync(
            this Compilation compilation,
            Document document,
            ExpressionSyntax expression
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
                return literal.Token.ValueText;
            }

            if (expression.IsKind(SyntaxKind.AddExpression))
            {
                var binaryExpression = expression as BinaryExpressionSyntax;

                var left = await TryGetStringValueAsync(compilation, document, binaryExpression.Left);
                if (!string.IsNullOrEmpty(left))
                {
                    var right = await TryGetStringValueAsync(compilation, document, binaryExpression.Right);
                    if (!string.IsNullOrEmpty(right))
                    {
                        return left + right;
                    }
                }

                return null;
            }

            var model = compilation.GetSemanticModel(expression.SyntaxTree);
            if (model == null)
            {
                return null;
            }

            var symbol = ModelExtensions.GetSymbolInfo(model, expression).Symbol;

            if (symbol == null)
            {
                return null;
            }

            var references = symbol.DeclaringSyntaxReferences.ToList();

            if (references.Count != 1)
            {
                return null;
            }

            //do searching for additional references, like additional assignments
            //in that case we skip this symbol usage due to uncertanity about REAL SQL body
            var globalReferences = (await SymbolFinder.FindReferencesAsync(
                symbol,
                document.Project.Solution,
                new List<Document> { document }.ToImmutableHashSet()
                )).ToList();

            if (globalReferences.Count != 1)
            {
                return null;
            }

            if (DetermineSymbolIsConstant(symbol))
            {
                //this symbol is compile-time constant value
                //we do not need to perform additional checks

                var globalDefinitionLocations = globalReferences[0].Definition.Locations.ToList();

                if (globalDefinitionLocations.Count != 1)
                {
                    //something strange happened!
                    return null;
                }
            }
            else
            {
                //it's not a constant
                //so we need to perform additional checks to guard ReSequel from the bugs like this:
                //string q0 = "fake body";
                //q0 = "print 0";
                //dbp.PrepareQuery(q0);
                //in this case ReSequel will check "fake body" instead of "print 0"
                //we should stop analysis in that cases, so we check the references for
                //current symbol (aka q0), and, if reference count > 1, we stop

                var globalLocations = globalReferences[0].Locations.ToList();

                if (globalLocations.Count != 1)
                {
                    return null;
                }

            }

            var defNode = await references[0].GetSyntaxAsync();

            var svList = new List<string>();
            var evcsList = defNode.DescendantNodes().OfType<EqualsValueClauseSyntax>().ToList();

            if (evcsList.Count == 0)
            {
                return null;
            }

            foreach (var evcs in evcsList)
            {
                var stringValue = await TryGetStringValueAsync(compilation, document, evcs.Value);
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }

                svList.Add(stringValue);
            }

            var result = string.Join(" ", svList);

            return result;

           //return null;
        }

        private static bool DetermineSymbolIsConstant(
            ISymbol symbol
            )
        {
            if (symbol.Kind == SymbolKind.Field && ((IFieldSymbol) symbol).HasConstantValue)
            {
                return true;
            }

            if (symbol.Kind == SymbolKind.Local && ((ILocalSymbol)symbol).HasConstantValue)
            {
                return true;
            }
            
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

            if (target.BaseType != null && !SymbolEqualityComparer.Default.Equals(target.BaseType, target))
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
