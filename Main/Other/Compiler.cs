using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compilation = Microsoft.CodeAnalysis.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading.Tasks;

namespace Main.Other
{
    /// <summary>
    /// should be stateless!
    /// </summary>
    public class Compiler
    {
        public async Task<List<ProjectArtifact>> CompileSolutionAsync(
            Solution solution,
            bool fixEncodingIssue
            )
        {
            if (solution == null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            var graph = solution.GetProjectDependencyGraph();

            var projectIdList = graph
                .GetTopologicallySortedProjects()
                .ToList()
                ;

            var artifacts = new List<ProjectArtifact>();
            foreach (var projectId in projectIdList)
            {
                var project = solution.GetProject(projectId);
                var compilation = await project.GetCompilationAsync();
                
                var diag = compilation.GetDiagnostics();

                var errors = diag.Where(j => j.Severity == DiagnosticSeverity.Error).ToList();
                if (errors.Count > 0)
                {
                    var errorMessage = string.Join(
                        Environment.NewLine,
                        errors.Select(j => j.ToString())
                        );

                    throw new InvalidOperationException(errorMessage);
                }


                if (fixEncodingIssue)
                {
                    compilation = FixEncodingIssue(compilation);
                }

                var pair = new ProjectArtifact(project, compilation);

                artifacts.Add(pair);
            }

            return
                artifacts;
        }

        /// <summary>
        /// https://github.com/dotnet/roslyn/issues/24045
        /// </summary>
        private Compilation FixEncodingIssue(
            Compilation compilation
            )
        {
            foreach (var tree in compilation.SyntaxTrees)
            {
                var encoded = CSharpSyntaxTree.Create(
                    tree.GetRoot() as CSharpSyntaxNode,
                    null,
                    string.Empty,
                    Encoding.UTF8
                    ).GetRoot();

                compilation = compilation.ReplaceSyntaxTree(tree, encoded.SyntaxTree);
            }

            return
                compilation;
        }
    }


}
