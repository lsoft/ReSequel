
using Main.Helper;
using Main.Inclusion.Found;
using Main.Logger;
using Main.ScanRelated;
using Main.WorkspaceWrapper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Main.Inclusion.Scanner
{
    public sealed class InclusionScanner : IInclusionScanner
    {
        public const string MuteEverywhereComment = "//ReSequel: MUTE next query everywhere";
        public const string MuteAtComment = "//ReSequel: MUTE next query at: ";
        public const string StringEmptyStatement = "string.Empty";

        private readonly ISolutionNameProvider _solutionNameProvider;
        private readonly Scan _scanScheme;

        private readonly IReadOnlyCollection<string> _generatorMethodSet;

        private readonly IReadOnlyCollection<string> _containerMethodSet;
        private readonly IReadOnlyCollection<string> _containerPropertySet;

        public InclusionScanner(
            ISolutionNameProvider solutionNameProvider,
            Scan scanScheme
            )
        {
            if (solutionNameProvider == null)
            {
                throw new ArgumentNullException(nameof(solutionNameProvider));
            }

            if (scanScheme == null)
            {
                throw new ArgumentNullException(nameof(scanScheme));
            }

            _solutionNameProvider = solutionNameProvider;
            _scanScheme = scanScheme;

            //make some optimization
            _generatorMethodSet = _scanScheme.GetAllGeneratorMethods();
            _containerMethodSet = _scanScheme.GetAllContainerMethods();
            _containerPropertySet = _scanScheme.GetAllContainerProperties();
        }

        public List<IFoundSqlInclusion> Scan(
            Microsoft.CodeAnalysis.Document document
            )
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var result = new List<IFoundSqlInclusion>();

            var duplicateChecker = GetDuplicateChecker();

            ProcessDocumentModel(
                document,
                duplicateChecker,
                result
                );

            return
                result.ToList();
        }


        public List<IFoundSqlInclusion> Scan(
            IWorkspaceWrapper subjectWorkspace,
            IProcessLogger processLogger
            )
        {
            if (subjectWorkspace == null)
            {
                throw new ArgumentNullException(nameof(subjectWorkspace));
            }

            if (processLogger == null)
            {
                throw new ArgumentNullException(nameof(processLogger));
            }

            var result = new List<IFoundSqlInclusion>();

            var duplicateChecker = GetDuplicateChecker();

            var before = DateTime.Now;

            var totalDocumentCount = subjectWorkspace.Workspace.CurrentSolution.Projects.Sum(p => p.Documents.Count());

            var graph = subjectWorkspace.Workspace.CurrentSolution.GetProjectDependencyGraph();

            var projectIdList = graph
                .GetTopologicallySortedProjects()
                .ToList()
                ;

            var processedDocumentCount = 0;
            foreach (var projectId in projectIdList)
            {
                var project = subjectWorkspace.Workspace.CurrentSolution.GetProject(projectId);

                foreach (var document in project.Documents)
                {
                    ProcessDocumentModel(
                        document,
                        duplicateChecker,
                        result
                        );

                    processedDocumentCount++;

                    processLogger.ShowProcessMessage(
                        "{0}/{1} documents processed",
                        processedDocumentCount,
                        totalDocumentCount
                        );
                }
            }


            var after = DateTime.Now;
            var diff = after - before;
            Console.WriteLine(diff);

            return
                result.ToList();
        }

        private void ProcessDocumentModel(
            Microsoft.CodeAnalysis.Document document,
            IDuplicateChecker duplicateChecker,
            List<IFoundSqlInclusion> result
            )
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (duplicateChecker == null)
            {
                throw new ArgumentNullException(nameof(duplicateChecker));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var model = document.GetSemanticModelAsync().Result;

            if (model == null)
            {
                //неизвестные проблемы доступа к семантической модели

                throw new InvalidOperationException("Cannot retrieve semantic model");
            }

            var diag = model.GetDiagnostics().ToList();

            var errors = diag.Where(j => j.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0)
            {
                //код не компилируется

                var errorMessage = string.Join(
                    Environment.NewLine,
                    errors.Select(j => j.ToString())
                    );

                throw new InvalidOperationException(errorMessage);
            }

            var tree = model.SyntaxTree;
            var root = tree.GetCompilationUnitRoot();

            var comments = root
                .DescendantTrivia()
                .Where(j => j.Kind() == SyntaxKind.SingleLineCommentTrivia)
                .Where(j => j.ToString() == MuteEverywhereComment)
                .Select(j => j.GetLocation().GetLineSpan().StartLinePosition.Line)
                .ToHashSet()
                ;

            var comments2 = (
                from trivia in root.DescendantTrivia()
                where trivia.Kind() == SyntaxKind.SingleLineCommentTrivia
                let triviaString = trivia.ToString()
                where triviaString.StartsWith(MuteAtComment)
                let tail = triviaString.Substring(MuteAtComment.Length).Trim()
                let parts = tail.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(j => j.Trim())
                where parts.Any(solutionName => Regex.IsMatch(_solutionNameProvider.SolutionName, solutionName.WildCardToRegular()))
                select trivia.GetLocation().GetLineSpan().StartLinePosition.Line
                ).ToHashSet();

            foreach (var comment in comments2)
            {
                comments.Add(comment);
            }

            ProcessAssignments(
              document,
              model,
              root,
              duplicateChecker,
              comments,
              ref result
              );

            ProcessInvocations(
                document,
                model,
                root,
                duplicateChecker,
                comments,
                ref result
                );

            ProcessGenerators(
                document,
                model,
                root,
                duplicateChecker,
                comments,
                ref result
                );
        }

        private void ProcessAssignments(
            Document document,
            SemanticModel model,
            CompilationUnitSyntax root,
            IDuplicateChecker duplicateChecker,
            IReadOnlyCollection<int> comments,
            ref List<IFoundSqlInclusion> result
            )
        {
            var assignments = (
                from assignment in root.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                select assignment)
                .ToList()
                ;

            foreach (AssignmentExpressionSyntax assignment in assignments)
            {
                //извлекаем всё, что потребуется впоследствии
                MemberAccessExpressionSyntax left = assignment.Left as MemberAccessExpressionSyntax;
                if (left == null)
                {
                    continue;
                }

                IdentifierNameSyntax identifier = left.Name as IdentifierNameSyntax;
                if (identifier == null)
                {
                    continue;
                }

                var propertyName = identifier?.Identifier.ValueText;
                if (propertyName == null)
                {
                    continue;
                }

                if (!_containerPropertySet.Contains(propertyName))
                {
                    continue;
                }

                var propertySymbol = model.GetSymbolInfo(identifier).Symbol as IPropertySymbol;
                if (propertySymbol == null)
                {
                    continue;
                }

                var classFullTypeName = propertySymbol.GetFullTypeNameOfContainingClass();

                if (string.IsNullOrEmpty(classFullTypeName))
                {
                    continue;
                }

                //теперь сканируем XML и ищем в нем что, что мы обнаружили в исходниках
                ScanProjectContainer @class;
                var classFound = _scanScheme.ContainerDictionary.TryGetValue(classFullTypeName, out @class);

                if (!classFound)
                {
                    //полное имя типа не совпадает
                    continue;
                }

                if (@class.Properties == null)
                {
                    continue;
                }

                if (@class.Properties.All(j => j.Name != propertyName))
                {
                    continue;
                }



                var right = assignment.Right;
                //LiteralExpressionSyntax right = assignment.Right as LiteralExpressionSyntax;
                //if (right == null)
                //{
                //    continue;
                //}

                if (!model.Compilation.TryGetStringValue(document, right, out var sqlBody))
                {
                    continue;
                }

                //var cv = model.GetConstantValue(right);
                //if (!cv.HasValue)
                //{
                //    continue;
                //}
                //var sqlBody = cv.Value?.ToString();

                var isMuted = comments.Contains(
                    assignment.Right.GetLocation().GetLineSpan().StartLinePosition.Line - 1
                    );

                var inclusion = new FoundSqlInclusion(
                    document,
                    right,
                    sqlBody,
                    isMuted
                    );

                if (!duplicateChecker.CheckForExistsAndAddIfNot(inclusion))
                {
                    result.Add(inclusion);
                }


            }
        }

        private void ProcessInvocations(
            Document document,
            SemanticModel model,
            CompilationUnitSyntax root,
            IDuplicateChecker duplicateChecker,
            IReadOnlyCollection<int> comments,
            ref List<IFoundSqlInclusion> result
            )
        {
            var invocations =
                (from invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                 select invocation)
                .ToList()
                ;

            foreach (InvocationExpressionSyntax invocation in invocations)
            {
                //извлекаем всё, что потребуется впоследствии

                MemberAccessExpressionSyntax memberAccessSyntax = invocation
                    .ChildNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .FirstOrDefault()
                    ;

                if (memberAccessSyntax == null)
                {
                    continue;
                }

                SimpleNameSyntax methodNameSyntax = memberAccessSyntax.Name;
                if (methodNameSyntax == null)
                {
                    continue;
                }

                string methodName = methodNameSyntax.Identifier.ValueText;

                //it's only an optimization
                //most invocation are not related with SQL execution,
                //so it's better to skip these invocation earlier
                if (!_containerMethodSet.Contains(methodName))
                {
                    continue;
                }

                var methodSymbol = model.GetSymbolInfo(methodNameSyntax).Symbol as IMethodSymbol;
                if (methodSymbol == null)
                {
                    continue;
                }

                var argumentListSyntax = invocation
                    .ChildNodes()
                    .OfType<ArgumentListSyntax>()
                    .FirstOrDefault()
                    ;

                if (argumentListSyntax == null)
                {
                    continue;
                }

                var classFullTypeName = methodSymbol.GetFullTypeNameOfContainingClass();

                if(string.IsNullOrEmpty(classFullTypeName))
                {
                    continue;
                }

                //теперь сканируем XML и ищем в нем что, что мы обнаружили в исходниках
                ScanProjectContainer @class;
                var classFound = _scanScheme.ContainerDictionary.TryGetValue(classFullTypeName, out @class);

                if (!classFound)
                {
                    //полное имя типа не совпадает
                    continue;
                }

                if (@class.Methods == null)
                {
                    continue;
                }

                foreach (ScanProjectContainerMethod method in @class.Methods)
                {
                    if (method.Name != methodName)
                    {
                        continue;
                    }
                    //этот метод имеет то же самое имя

                    if (method.Arguments.Length == 0)
                    {
                        //метод без аргументов
                        continue;
                    }

                    if (method.Arguments.Length != argumentListSyntax.Arguments.Count)
                    {
                        //этот метод имеет другое количество аргументов, чем в XML
                        continue;
                    }

                    if (!method.Arguments.Any(j => j.ContainsSql))
                    {
                        //этот метод из XML не имеет признака содержания SQL
                        //это ошибка составления XML, но мы просто пропустим этот
                        //метод молча, без генерации ошибки
                        continue;
                    }

                    //проверяем все аргументы на соответствие схеме
                    var xmlArguments = method.Arguments.ToList();

                    var argumentTypeCheck = CheckArgumentTypes(
                        methodSymbol,
                        xmlArguments
                        );

                    if (!argumentTypeCheck)
                    {
                        //хотя бы 1 аргумент не соотвествует типу из XML
                        continue;
                    }

                    //все проверки прошли успешно, генерируем ответ
                    for (var argIndex = 0; argIndex < xmlArguments.Count; argIndex++)
                    {
                        var xmlArgument = xmlArguments[argIndex];
                        if (!xmlArgument.ContainsSql)
                        {
                            continue;
                        }

                        ArgumentSyntax argumentSyntax = argumentListSyntax.Arguments[argIndex];

                        if (!model.Compilation.TryGetStringValue(document, argumentSyntax.Expression, out var sqlBody))
                        {
                            continue;
                        }

                        //var cv = model.GetConstantValue(argumentSyntax.Expression);
                        //if (!cv.HasValue)
                        //{
                        //    continue;
                        //}
                        //var sqlBody = cv.Value?.ToString();

                        var isMuted = comments.Contains(
                            argumentSyntax.Expression.GetLocation().GetLineSpan().StartLinePosition.Line - 1
                            );

                        var inclusion = new FoundSqlInclusion(
                            document,
                            argumentSyntax,

                            sqlBody,
                            isMuted
                            );

                        if (!duplicateChecker.CheckForExistsAndAddIfNot(inclusion))
                        {
                            result.Add(inclusion);
                        }
                    }
                }
            }
        }

        private void ProcessGenerators(
            Document document,
            SemanticModel model,
            CompilationUnitSyntax root,
            IDuplicateChecker duplicateChecker,
            IReadOnlyCollection<int> comments,
            ref List<IFoundSqlInclusion> result
            )
        {
            //if (!document.Name.Contains("Program.cs"))
            //    return;

            var members = root
                .DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .ToList()
                ;

            var dict0 = new Dictionary<ISymbol, List<MemberAccessExpressionSyntax>>();
            foreach (var member in members)
            {
                var e = member.Expression as IdentifierNameSyntax;

                if (e == null)
                {
                    continue;
                }

                //var fullTypeName = symbol.GetFullTypeName2();
                var fullTypeName = model.GetFullTypeName(e);

                if (!_scanScheme.GeneratorDictionary.ContainsKey(fullTypeName))
                {
                    continue;
                }

                var symbol = model.GetSymbolInfo(e).Symbol;

                if (symbol == null)
                {
                    continue;
                }

                if (!dict0.ContainsKey(symbol))
                {
                    dict0[symbol] = new List<MemberAccessExpressionSyntax>();
                }

                dict0[symbol].Add(member);
            }

            var dict1 = new Dictionary<ISymbol, List<StatementSyntax>>();
            foreach (var pair in dict0)
            {
                var symbol = pair.Key;

                dict1.Add(symbol, new List<StatementSyntax>());

                foreach (var reff in pair.Value.OrderBy(k => k.Span.End))
                {
                    SyntaxNode node = reff;

                    //while(node.Parent != null && node.DescendantNodesAndTokens().Last().Kind() != SyntaxKind.SemicolonToken)
                    while(node.Parent != null && !(node is StatementSyntax))
                    {
                        node = node.Parent;
                    }

                    if (node is StatementSyntax)
                    {
                        dict1[symbol].Add(node as StatementSyntax);
                    }
                }
            }

            foreach (var pair in dict1)
            {
                var symbol = pair.Key;

                var fullTypeName = symbol.GetFullTypeName();

                if(string.IsNullOrEmpty(fullTypeName))
                {
                    continue;
                }

                ScanProjectGenerator generatorType;
                if (!_scanScheme.GeneratorDictionary.TryGetValue(fullTypeName, out generatorType))
                {
                    continue;
                }

                var generator = new Generator.Generator();

                foreach (var reff in pair.Value.OrderBy(k => k.Span.End))
                {
                    var invocations =
                        (from invocation in reff.DescendantNodes().OfType<InvocationExpressionSyntax>()
                         orderby invocation.Span.End
                         select invocation)
                        .ToList()
                        ;

                    foreach (InvocationExpressionSyntax invocation in invocations)
                    {
                        MemberAccessExpressionSyntax memberAccessSyntax = invocation
                            .ChildNodes()
                            .OfType<MemberAccessExpressionSyntax>()
                            .OrderBy(j => j.Span.End)
                            .FirstOrDefault()
                            ;

                        if (memberAccessSyntax == null)
                        {
                            continue;
                        }

                        SimpleNameSyntax methodNameSyntax = memberAccessSyntax.Name;
                        if (methodNameSyntax == null)
                        {
                            continue;
                        }

                        string methodName = methodNameSyntax.Identifier.ValueText;

                        //it's only an optimization
                        //most invocation are not related with SQL execution,
                        //so it's better to skip these invocation earlier
                        if (!_generatorMethodSet.Contains(methodName))
                        {
                            continue;
                        }

                        var methodSymbol = model.GetSymbolInfo(methodNameSyntax).Symbol as IMethodSymbol;
                        if (methodSymbol == null)
                        {
                            continue;
                        }

                        var argumentListSyntax = invocation
                            .ChildNodes()
                            .OfType<ArgumentListSyntax>()
                            .FirstOrDefault()
                            ;

                        if (argumentListSyntax == null)
                        {
                            continue;
                        }

                        if(generatorType.IsItSqlBodyMethod(methodName))
                        {
                            var argumentSyntax = argumentListSyntax.Arguments[0];

                            if (!model.Compilation.TryGetStringValue(document, argumentSyntax.Expression, out var queryTemplate))
                            {
                                continue;
                            }

                            //var cv = model.GetConstantValue(argumentSyntax.Expression);
                            //if (!cv.HasValue)
                            //{
                            //    continue;
                            //}
                            //var queryTemplate = cv.Value?.ToString();

                            generator.WithQuery(queryTemplate);
                        }

                        if (generatorType.IsItOptionMethod(methodName))
                        {
                            var argValues = new List<string>();
                            foreach (ArgumentSyntax argumentSyntax in argumentListSyntax.Arguments)
                            {
                                var ase = argumentSyntax.Expression.ToFullString();
                                ase = ase.Replace(" ", "").Trim();

                                if (StringComparer.InvariantCultureIgnoreCase.Compare(ase, StringEmptyStatement) == 0)
                                {
                                    argValues.Add(string.Empty);
                                }
                                else
                                {
                                    var cv = model.GetConstantValue(argumentSyntax.Expression);
                                    if (!cv.HasValue)
                                    {
                                        //incorrect method, one of its argument is not a const string
                                        //we should skip this generator usage completely
                                        goto nextGenerator; //yes, yes, it is goto!!! muhahaha!!! come with me to the dark side of programming!
                                    }

                                    var argValue = cv.Value?.ToString();
                                    argValues.Add(argValue);
                                }
                            }

                            generator.DeclareOption(argValues);
                        }
                    }
                }

                if (!generator.IsEmpty)
                {
                    generator.DoFixing();

                    var isMuted = comments.Contains(
                        pair.Value.OrderBy(k => k.Span.Start).First().GetLocation().GetLineSpan().StartLinePosition.Line - 1
                        );

                    var inclusion = new FoundSqlInclusion(
                        document,
                        pair.Value.First(),

                        generator,
                        isMuted
                        );

                    if (!duplicateChecker.CheckForExistsAndAddIfNot(inclusion))
                    {
                        result.Add(inclusion);
                    }
                }

            nextGenerator:
                int g = 0; //next iteration

            }
        }

        private static bool CheckArgumentTypes(
            IMethodSymbol methodSymbol,
            List<ScanProjectContainerMethodArgument> xmlArguments
            )
        {
            var argumentCheckPasses = true;
            for (var cc = 0; cc < xmlArguments.Count; cc++)
            {
                var xmla = xmlArguments[cc];
                IParameterSymbol a = methodSymbol.Parameters[cc];

                ITypeSymbol roslynType = a.Type;
                if (!roslynType.CanBeCastedTo(xmla.Type))
                {
                    argumentCheckPasses = false;
                    break;
                }

            }

            return argumentCheckPasses;
        }

        #region duplicate checker

        private IDuplicateChecker GetDuplicateChecker()
        {
            return
                new DuplicateChecker();
        }

        private interface IDuplicateChecker
        {
            bool CheckForExistsAndAddIfNot(IFoundSqlInclusion inclusion);
        }

        private class DuplicateChecker : IDuplicateChecker
        {
            private readonly HashSet<IFoundSqlInclusion> _set = new HashSet<IFoundSqlInclusion>(FoundSqlInclusionEqualityComparer.Instance);

            public bool CheckForExistsAndAddIfNot(IFoundSqlInclusion inclusion)
            {
                if (inclusion == null)
                {
                    throw new ArgumentNullException(nameof(inclusion));
                }

                var result = _set.Contains(inclusion);

                if (!result)
                {
                    _set.Add(inclusion);
                }

                return
                    result;
            }
        }

        #endregion
    }

}
